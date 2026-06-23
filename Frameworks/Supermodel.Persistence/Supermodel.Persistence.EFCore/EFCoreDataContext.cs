using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pluralize.NET.Core;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Validations;
using Supermodel.Persistence.DataContext;
using Supermodel.Persistence.Entities;
using Supermodel.Persistence.Entities.ValueTypes;
using Supermodel.Persistence.Repository;
using Supermodel.ReflectionMapper;

namespace Supermodel.Persistence.EFCore;

public abstract class EFCoreDataContext : DbContext, IDataContext
{
    #region Constructors
    protected EFCoreDataContext(string connectionString, IRepoFactory? customRepoFactory = null)
    {
        ConnectionString = connectionString;
        if (customRepoFactory != null) CustomRepoFactoryList.Add(customRepoFactory);
    }
    #endregion

    #region Overrides and Virtual Methods
    //Returns the assemblies containing entities and entity config classes for this unit of work. Default returns AppDomain.CurrentDomain.GetAssemblies(). Override it for finer control 
    protected virtual Assembly[] GetDomainEntitiesAssemblies() { return AppDomain.CurrentDomain.GetAllAssemblies(); }
    protected override async void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var assemblies = GetDomainEntitiesAssemblies();
            
        //Set up entities that implement IEntity
        var pluralizer = new Pluralizer();
        foreach (var assembly in assemblies)
        {
            //We specifically skip Microsoft.Data.SqlClient assembly because of the
            //problem in .net 8.0. See https://github.com/dotnet/runtime/issues/86969
            //If you ever change this, search solution for 23ec7fc2d6eaa4a5 (PublicKeyToken)
            if (assembly.FullName == "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5") continue;

            Type[] typesInAssembly;
            try { typesInAssembly = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException) { continue; }

            foreach (var type in typesInAssembly)
            {
                if (type.IsAbstract) continue;
                if (typeof(IEntity).IsAssignableFrom(type) && !IsDbSetTypeAProxy(type)) 
                {
                    var etb = modelBuilder.Entity(type);
                    etb.ToTable(pluralizer.Pluralize(type.Name));

                    //Add unique username index for classes that derive from UserEntity
                    if (ReflectionHelper.IsClassADerivedFromClassB(type, typeof(UserEntity<,>))) etb.HasIndex("Username").IsUnique();
                }
            }
        }

        //Set up configurations that are derived from IEntityTypeConfiguration
        foreach (var assembly in assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        //register value type properties as owned
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()).ToList())
        {
            if (typeof(IValueObject).IsAssignableFrom(relationship.PrincipalEntityType.ClrType)) 
            {
                var ownerType = relationship.DependentToPrincipal!.DeclaringType.ClrType;
                var ownedType = relationship.DependentToPrincipal.ClrType;
                var ownedName = relationship.DependentToPrincipal.Name;
                var etb = modelBuilder.Entity(ownerType);
                //etb.OwnsOne(ownedType, ownedName);       //this is for pre EF 8.0 versions of Supermodel
                etb.ComplexProperty(ownedType, ownedName); //this is only good for EF 8.0 and would introduce a breaking change
            }
        }

        //This sets up no cascading deletes/updates
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        //This is to make sure we can delete an entity with owned properties. We set up cascading deletes for these
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().Where(e => e.IsOwned()).SelectMany(e => e.GetForeignKeys())) 
        {
            relationship.DeleteBehavior = DeleteBehavior.Cascade;
        }

        //Seed data
        try
        {
            await SeedDataWithModelBuilderAsync(modelBuilder);
        }
        // ReSharper disable once RedundantCatchClause
        #pragma warning disable 168
        catch (Exception ex) //This is not redundant, this is to catch exceptions in void async method
        #pragma warning restore 168
        {
            Environment.Exit(1);
        }
    }
    public virtual Task SeedDataAsync()
    {
        return Task.CompletedTask;
    }
    protected virtual Task SeedDataWithModelBuilderAsync(ModelBuilder modelBuilder)
    {
        return Task.CompletedTask;
    }
    #endregion
        
    #region Methods
    public IQueryable<TEntity> Items<TEntity>() where TEntity : class, IEntity, new()
    {
        return Set<TEntity>();
    }

    public IDataRepo CreateRepoForRuntimeType(Type modelType)
    {
        modelType = GetDbSetTypeOfPossibleProxy(modelType);
        var repo = (IDataRepo?)this.ExecuteGenericMethod(nameof(CreateRepo), [modelType]);
        if (repo == null) throw new SupermodelException("repo == null. This should never happen");
        return repo;
    }
    public IDataRepo<TEntity> CreateRepo<TEntity>() where TEntity : class, IEntity, new()
    {
        foreach (var customFactory in CustomRepoFactoryList)
        {
            var repo = customFactory.CreateRepo<TEntity>();
            if (repo != null) return repo;
        }
        return new EFCoreSimpleDataRepo<TEntity>();        
    }
    protected List<IRepoFactory> CustomRepoFactoryList { get; } = [];

    public IDataContextTransaction BeginTransaction()
    {
        return new EFCoreTransaction(Database.BeginTransaction());
    }

    public TEntity CloneDetached<TEntity>(TEntity entity) where TEntity : class, IEntity, new()
    {
        var values = Entry(entity).CurrentValues.Clone();
        var newEntity = new TEntity();
        Entry(newEntity).CurrentValues.SetValues(values);
        Entry(newEntity).State = EntityState.Detached;
        return newEntity;
    }
    #endregion

    #region Dispose Methods
    public override async ValueTask DisposeAsync()
    {
        if (CommitOnDispose && !IsReadOnly && !IsCompletedAndFinalized) await SaveChangesAsync();
        await base.DisposeAsync();
    }
    #endregion

    #region Save Changes Methods
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        try
        {
            if (!CommitOnDispose || IsReadOnly || IsCompletedAndFinalized) return 0;
            await RunBeforeSaveAndValidationMethodsAsync();
            return await base.SaveChangesAsync(true, cancellationToken);
        }
        // ReSharper disable RedundantCatchClause
        #pragma warning disable 168
        catch (Exception ex)
        {
            throw; //we need this for debugging
        }
        #pragma warning restore 168
        // ReSharper restore RedundantCatchClause
    }

    protected async Task RunBeforeSaveAndValidationMethodsAsync()
    {
        var modifiedEntities = DetectModifiedEntities();
        await RunBeforeSaveMethodsAsync(modifiedEntities);
        await RunValidationsAsync(modifiedEntities);
    }
    protected List<EntityEntry> DetectModifiedEntities()
    {
        ChangeTracker.DetectChanges();
        var modifiedEntities = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged && x.State != EntityState.Detached).ToList();
        return modifiedEntities;
    }
    protected async Task RunBeforeSaveMethodsAsync(List<EntityEntry> modifiedEntities)
    {
        foreach (var entity in modifiedEntities)
        {
            if (entity.Entity is IEntity iEntity)
            {
                var operation = entity.State switch
                {
                    EntityState.Added => OperationEnum.Add,
                    EntityState.Deleted => OperationEnum.Delete,
                    EntityState.Modified => OperationEnum.Update,
                    _ => throw new Exception("Invalid EntityState. This should never happen")
                };

                await iEntity.BeforeSaveAsync(operation);
            }
        }
    }
    protected async Task RunValidationsAsync(List<EntityEntry> modifiedEntities)
    {
        if (ValidateOnSaveEnabled)
        {
            foreach (var entity in modifiedEntities)
            {
                if (entity.State == EntityState.Added || entity.State == EntityState.Modified)
                {
                    var validationContext = new ValidationContext(entity.Entity);
                    await AsyncValidator.ValidateObjectAsync(entity.Entity, validationContext, true);
                }
            }
        }
    }

    public async Task<int> FinalSaveChangesAsync(CancellationToken cancellationToken = new())
    {
        int result;
        try
        {
            result = await SaveChangesAsync(cancellationToken);
        }
        finally
        {
            MakeCompletedAndFinalized();
        }
        return result;
    }
    #endregion

    #region Private Helpers
    private static Type GetDbSetTypeOfPossibleProxy(Type dataSetType)
    {
        if (dataSetType == null) throw new ArgumentNullException(nameof(dataSetType));
        if (IsDbSetTypeAProxy(dataSetType)) dataSetType = dataSetType.BaseType!; //for EF Core Lazy Loading
        return dataSetType;
    }
    private static bool IsDbSetTypeAProxy(Type dataSetType)
    {
        // ReSharper disable once PossibleNullReferenceException
        return dataSetType.FullName!.StartsWith("Castle.Proxies.");
    }
    #endregion

    #region Methods Marked Obsolete
    //we need these blocking methods to work for migrations but we override them and mark them obsolete and error producing so that they are not used
        
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    [Obsolete("SaveChanges is not supported in Supermodel, please use SaveChangesAsync instead.", true)]
    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    [Obsolete("SaveChanges is not supported in Supermodel, please use SaveChangesAsync instead.", true)]
    public override int SaveChanges(bool acceptAllChangesOnSuccess) 
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    [Obsolete("SaveChangesAsync with acceptAllChangesOnSuccess parameter is not supported in Supermodel, please use SaveChangesAsync without acceptAllChangesOnSuccess instead.", true)]
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        throw new SupermodelException("SaveChangesAsync(acceptAllChangesOnSuccess): Supermodel does not support SaveChangesAsync with acceptAllChangesOnSuccess parameter");
    }
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    #endregion

    #region Properties
    public bool CommitOnDispose { get; set; } = true;
        
    public bool IsReadOnly { get; protected set;}
    public void MakeReadOnly()
    {
        ValidateOnSaveEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        IsReadOnly = true;
    }

    public bool IsCompletedAndFinalized { get; protected set; }
    public void MakeCompletedAndFinalized()
    {
        IsCompletedAndFinalized = true;
    }

    public ConcurrentDictionary<string, object?> CustomValues { get; } = new();

    public string ConnectionString { get; }

    public bool LoadReadOnlyEntitiesAsNoTracking 
    { 
        get => _loadReadOnlyEntitiesAsNoTracking;
        set
        {
            if (_loadReadOnlyEntitiesAsNoTracking == value) return;
            _loadReadOnlyEntitiesAsNoTracking = value;
            if (value)
            {
                if (!IsReadOnly) throw new SupermodelException("Setting 'LoadReadOnlyEntitiesAsNoTracking=true' for Read/Write context is not allowed");
                ChangeTracker.LazyLoadingEnabled = false;
                ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
            else
            {
                ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        } 
    }
    private bool _loadReadOnlyEntitiesAsNoTracking;

    public bool ValidateOnSaveEnabled { get; set; } = true;
}
#endregion