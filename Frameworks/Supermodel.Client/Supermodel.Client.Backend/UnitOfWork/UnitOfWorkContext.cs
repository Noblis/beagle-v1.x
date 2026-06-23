using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.DataContext.Sqlite;
using Supermodel.Client.Backend.DataContext.WebApi;
using Supermodel.Client.Backend.Models;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.LogicalContext;
using Supermodel.Encryptor;

namespace Supermodel.Client.Backend.UnitOfWork;

//Shortcuts for the most often used Context methods
public static class UnitOfWorkContext
{
    #region Methods and Properties
    public static Dictionary<string, object> CustomValues => UnitOfWorkContextCore.CurrentDataContext.CustomValues;
    public static Task ResetDatabaseAsync()
    {
        if (!(UnitOfWorkContextCore.CurrentDataContext is SqliteDataContext)) throw new SupermodelException("ResetDatabaseAsync() is only valid for SqliteDataContext");
        return ((SqliteDataContext)UnitOfWorkContextCore.CurrentDataContext).ResetDatabaseAsync();
    }
    public static Task SaveChangesAsync()
    {
        if (!(UnitOfWorkContextCore.CurrentDataContext is IWritableDataContext)) throw new SupermodelException("SaveChangesAsync() is only valid for IWritableDataContext");
        return ((IWritableDataContext)UnitOfWorkContextCore.CurrentDataContext).SaveChangesAsync();
    }
    public static Task FinalSaveChangesAsync()
    {
        if (!(UnitOfWorkContextCore.CurrentDataContext is IWritableDataContext)) throw new SupermodelException("FinalSaveChangesAsync() is only valid for IWritableDataContext");
        return ((IWritableDataContext)UnitOfWorkContextCore.CurrentDataContext).FinalSaveChangesAsync();
    }
    public static void DetectUpdates()
    {
        if (!(UnitOfWorkContextCore.CurrentDataContext is IWritableDataContext)) throw new SupermodelException("DetectUpdates() is only valid for IWritableDataContext");
        ((IWritableDataContext)UnitOfWorkContextCore.CurrentDataContext).DetectAllUpdates();
    }
    public static bool CommitOnDispose
    {
        get => UnitOfWorkContextCore.CurrentDataContext.CommitOnDispose;
        set => UnitOfWorkContextCore.CurrentDataContext.CommitOnDispose = value;
    }
    public static AuthHeader? AuthHeader
    {
        get
        {
            if (!(UnitOfWorkContextCore.CurrentDataContext is IWebApiAuthorizationContext)) throw new SupermodelException("AuthHeader is only accessible for IWebApiAuthorizationContext");
            return ((IWebApiAuthorizationContext)UnitOfWorkContextCore.CurrentDataContext).AuthHeader;
        }
        set
        {
            if (!(UnitOfWorkContextCore.CurrentDataContext is IWebApiAuthorizationContext)) throw new SupermodelException("AuthorizationHeader is only accessible for IWebApiAuthorizationContext");
            ((IWebApiAuthorizationContext)UnitOfWorkContextCore.CurrentDataContext).AuthHeader = value;
        }
    }
    public static Task<LoginResult> ValidateLoginAsync<TModel>() where TModel : class, IModel
    {
        if (!(UnitOfWorkContextCore.CurrentDataContext is IWebApiAuthorizationContext)) throw new SupermodelException("AuthorizationHeader is only accessible for IWebApiAuthorizationContext");
        return ((IWebApiAuthorizationContext)UnitOfWorkContextCore.CurrentDataContext).ValidateLoginAsync<TModel>();
    }
    public static int CacheAgeToleranceInSeconds
    {
        get
        {
            if (!(UnitOfWorkContextCore.CurrentDataContext is ICachedDataContext)) throw new SupermodelException("CacheAgeToleranceInSeconds is only accessible for ICachedDataContext");
            return ((ICachedDataContext)UnitOfWorkContextCore.CurrentDataContext).CacheAgeToleranceInSeconds;
        }
        set
        {
            if (!(UnitOfWorkContextCore.CurrentDataContext is ICachedDataContext)) throw new SupermodelException("CacheAgeToleranceInSeconds is only accessible for ICachedDataContext");
            ((ICachedDataContext)UnitOfWorkContextCore.CurrentDataContext).CacheAgeToleranceInSeconds = value;
        }
    }
    public static Task PurgeCacheAsync(int? cacheExpirationAgeInSeconds = null, Type? modelType = null)
    {
        if (!(UnitOfWorkContextCore.CurrentDataContext is ICachedDataContext)) throw new SupermodelException("CacheAgeToleranceInSeconds is only accessible for ICachedDataContext");
        return ((ICachedDataContext)UnitOfWorkContextCore.CurrentDataContext).PurgeCacheAsync(cacheExpirationAgeInSeconds, modelType);
    }
    #endregion
}
    
public static class UnitOfWorkContext<TDataContext> where TDataContext : class, IDataContext, new()
{
    #region Methods and Properties
    public static TDataContext PopDbContext()
    {
        return (TDataContext)UnitOfWorkContextCore.PopDbContext();
    }
    public static void PushDbContext(TDataContext context)
    {
        UnitOfWorkContextCore.PushDbContext(context);
    }
    public static int StackCount => UnitOfWorkContextCore.StackCount;
    public static TDataContext CurrentDataContext => (TDataContext)UnitOfWorkContextCore.CurrentDataContext;
    public static bool HasDbContext()
    {
        return StackCount > 0;
    }
    #endregion
}

public static class UnitOfWorkContextCore
{
    #region Methods and Properties
    public static IDataContext PopDbContext()
    {
        try
        {
            _contextStackImmutable = _contextStackImmutable.Pop(out var context);
            return context;

        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("Stack is empty");
        }
    }
    public static void PushDbContext(IDataContext context)
    {
        _contextStackImmutable = _contextStackImmutable.Push(context);
    }
    public static int StackCount => _contextStackImmutable.Count();
    public static IDataContext CurrentDataContext
    {
        get
        {
            try
            {
                return _contextStackImmutable.Peek();
            }
            catch (InvalidOperationException)
            {
                throw new SupermodelException("Current UnitOfWork does not exist. All database access operations must be wrapped in 'using(new UnitOfWork())'");
            }
        }
    }
    #endregion

    #region Private variables
    // ReSharper disable once InconsistentNaming
    private static ImmutableStack<IDataContext> _contextStackImmutable
    {
        get
        {
            var contextStack = DataCallContext.LogicalGetData(SupermodelDataContextStack) as ImmutableStack<IDataContext>;
            return contextStack ?? ImmutableStack.Create<IDataContext>();
        }
        set => DataCallContext.LogicalSetData(SupermodelDataContextStack, value);
    }
    public const string SupermodelDataContextStack = "SupermodelDataContextStack";
    #endregion
}