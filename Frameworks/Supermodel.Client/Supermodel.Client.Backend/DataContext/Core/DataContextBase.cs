using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Supermodel.Client.Backend.Exceptions;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.Repository;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Validations;

namespace Supermodel.Client.Backend.DataContext.Core;

public abstract class DataContextBase : IQueryableReadableDataContext, IWritableDataContext
{
    #region Contructors
    protected DataContextBase()
    {
        CommitOnDispose = true;
        IsReadOnly = false;
        IsCompletedAndFinalized = false;
            
        ManagedModels = new List<ManagedModel>();
        PendingActions = new List<PendingAction>();

        CustomValues = new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    public static string GetModelTypeLogicalName<TModel>() where TModel : class, IModel
    {
        return GetModelTypeLogicalName(typeof(TModel));
    }
    public static string GetModelTypeLogicalName(Type? type)
    {
        if (!typeof(IModel).IsAssignableFrom(type)) throw new SupermodelException("GetModelTypeLogicalName can only be called for types that implement IModel");
        //var restUrlAttribute = type.GetCustomAttributes(typeof(RestUrlAttribute), true).FirstOrDefault() as RestUrlAttribute;
        var restUrlAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(RestUrlAttribute), true).FirstOrDefault() as RestUrlAttribute;
        return restUrlAttribute == null ? type.Name : restUrlAttribute.Url;
    }
    public void DetectAllUpdates()
    {
        //remove all updates that are already in PendingActions
        PendingActions.RemoveAll(x => x.Operation == PendingAction.OperationEnum.Update);

        DetectNewUpdates();
    }
    public void DetectNewUpdates()
    {
        //detect new updates and put them in PendingActions
        foreach (var managedModel in ManagedModels.Where(managedModel => managedModel.NeedsUpdating()))
        {
            if (!PendingActions.Any(x => x.Operation == PendingAction.OperationEnum.Update && x.ModelType == managedModel.Model.GetType() && x.ModelId == managedModel.Model.Id && x.OriginalModelId == managedModel.OriginalModelId ))
            {
                PendingActions.Add(new PendingAction
                {
                    Operation = PendingAction.OperationEnum.Update,
                    ModelType = managedModel.Model.GetType(),
                    ModelId = managedModel.Model.Id,
                    OriginalModelId = managedModel.OriginalModelId,
                    Model = managedModel.Model,
                    DelayedValue = null,
                    SearchBy = null,
                    Skip = null,
                    Take = null,
                    SortBy = null
                }.Validate());
            }
        }
    }
    #endregion

    #region DataContext Reads
    public virtual async Task<TModel> GetByIdAsync<TModel>(long id) where TModel : class, IModel, new()
    {
        var model = await GetByIdOrDefaultAsync<TModel>(id);
        if (model == null) throw new SupermodelException("GetByIdAsync(id): no object exists with id = " + id);
        return model;
    }
    public abstract Task<TModel?> GetByIdOrDefaultAsync<TModel>(long id) where TModel : class, IModel, new();
    public abstract Task<List<TModel>> GetAllAsync<TModel>(int? skip = null, int? take = null) where TModel : class, IModel, new();
    public abstract Task<long> GetCountAllAsync<TModel>(int? skip = null, int? take = null) where TModel : class, IModel, new();
    #endregion

    #region DataContext Delayed Reads
    public void DelayedGetById<TModel>(out DelayedModel<TModel> model, long id) where TModel : class, IModel, new()
    {
        model = new DelayedModel<TModel>();
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.DelayedGetById,
            ModelType = typeof(TModel),
            ModelId = id,
            OriginalModelId = 0,
            Model = null,
            DelayedValue = model,
            SearchBy = null,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    public void DelayedGetByIdOrDefault<TModel>(out DelayedModel<TModel> model, long id) where TModel : class, IModel, new()
    {
        model = new DelayedModel<TModel>();
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.DelayedGetByIdOrDefault,
            ModelType = typeof(TModel),
            ModelId = id,
            OriginalModelId = 0,
            Model = null,
            DelayedValue = model,
            SearchBy = null,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    public void DelayedGetAll<TModel>(out DelayedModels<TModel> models) where TModel : class, IModel, new()
    {
        models = new DelayedModels<TModel>();
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.DelayedGetAll,
            ModelType = typeof(TModel),
            ModelId = 0,
            OriginalModelId = 0,
            Model = null,
            DelayedValue = models,
            SearchBy = null,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    public void DelayedGetCountAll<TModel>(out DelayedCount count) where TModel : class, IModel, new()
    {
        count = new DelayedCount();
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.DelayedGetCountAll,
            ModelType = typeof(TModel),
            ModelId = 0,
            OriginalModelId = 0,
            Model = null,
            DelayedValue = count,
            SearchBy = null,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    #endregion

    #region DataContext Queries
    public abstract Task<List<TModel>> GetWhereAsync<TModel>(object? searchBy, string? sortBy = null, int? skip = null, int? take = null) where TModel : class, IModel, new();
    public abstract Task<long> GetCountWhereAsync<TModel>(object? searchBy, int? skip = null, int? take = null) where TModel : class, IModel, new();
    #endregion

    #region DataContext Delayed Queries
    public void DelayedGetWhere<TModel>(out DelayedModels<TModel> models, object? searchBy, string? sortBy = null, int? skip = null, int? take = null) where TModel : class, IModel, new()
    {
        models = new DelayedModels<TModel>();
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.DelayedGetWhere,
            ModelType = typeof(TModel),
            ModelId = 0,
            OriginalModelId = 0,
            Model = null,
            DelayedValue = models,
            SearchBy = searchBy,
            Skip = skip,
            Take = take,
            SortBy = sortBy
        }.Validate());
    }
    public void DelayedGetCountWhere<TModel>(out DelayedCount count, object? searchBy) where TModel : class, IModel, new()
    {
        count = new DelayedCount();

        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.DelayedGetCountWhere,
            ModelType = typeof(TModel),
            ModelId = 0,
            OriginalModelId = 0,
            Model = null,
            DelayedValue = count,
            SearchBy = searchBy,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    #endregion

    #region DataContext Writes
    public void Add<TModel>(TModel model) where TModel : class, IModel, new()
    {
        var operation = model.Id == 0 ? PendingAction.OperationEnum.GenerateIdAndAdd : PendingAction.OperationEnum.AddWithExistingId;
        PendingActions.Add(new PendingAction
        {
            Operation = operation,
            ModelType = typeof(TModel),
            ModelId = model.Id,
            OriginalModelId = 0,
            Model = model,
            DelayedValue = null,
            SearchBy = null,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    public void Delete<TModel>(TModel model) where TModel : class, IModel, new()
    {
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.Delete,
            ModelType = typeof (TModel),
            ModelId = model.Id,
            OriginalModelId = model.Id,
            Model = model,
            DelayedValue = null,
            SearchBy = null,
            Skip = null,
            Take = null,
            SortBy = null
        }.Validate());
    }
    public void ForceUpdate<TModel>(TModel model) where TModel : class, IModel, new()
    {
        var managedModel = ManagedModels.SingleOrDefault(x => x.Model == model);
            
        if (managedModel == null) ManagedModels.Add(new ManagedModel(model){ ForceUpdate = true });
        else managedModel.ForceUpdate = true;
    }
    #endregion

    #region IAsyncDisposable implemetation
    public async ValueTask DisposeAsync()
    {
        if (IsCompletedAndFinalized) return;

        if (IsReadOnly || !CommitOnDispose) PendingActions.RemoveAll(x => !x.IsReadOnlyAction);
        else DetectNewUpdates();

        if (PendingActions.Any()) await FinalSaveChangesAsync();
    }
    #endregion
        
    #region DataContext Configuration
    public bool CommitOnDispose { get; set; }
    public bool IsReadOnly { get; protected set; }
    public void MakeReadOnly()
    {
        IsReadOnly = true;
    }
    public bool IsCompletedAndFinalized { get; protected set; }
    public void MakeCompletedAndFinalized()
    {
        IsCompletedAndFinalized = true;
    }
    #endregion

    #region Context RepoFactory
    public virtual IDataRepo<TModel> CreateRepo<TModel>() where TModel : class, IModel, new()
    {
        if (CustomRepoFactoryList != null)
        {
            foreach (var customFactory in CustomRepoFactoryList)
            {
                var repo = customFactory.CreateRepo<TModel>();
                if (repo != null) return repo;
            }
        }
        return new DataRepo<TModel>();
    }
    protected List<IRepoFactory>? CustomRepoFactoryList => null;

    #endregion

    #region DataContext Save Changes
    public async Task FinalSaveChangesAsync()
    {
        try
        {
            await SaveChangesAsync(true);
        }
        finally
        {
            MakeCompletedAndFinalized();
        }
    }
    public async Task SaveChangesAsync()
    {
        await SaveChangesAsync(false);
    }
    protected virtual async Task SaveChangesAsync(bool isFinal)
    {
        if (IsCompletedAndFinalized) return;

        if (IsReadOnly || !CommitOnDispose) PendingActions.RemoveAll(x => !x.IsReadOnlyAction);
        else DetectNewUpdates();

        // ReSharper disable once SimplifyLinqExpression
        if (!PendingActions.Any(x => !x.Disabled)) return;

        //Run BeforeSave for all Models about to be saved
        foreach (var pendingAction in PendingActions.Where(x => !x.Disabled))
        {
            if (pendingAction.Operation == PendingAction.OperationEnum.AddOrUpdate || 
                pendingAction.Operation == PendingAction.OperationEnum.AddWithExistingId ||
                pendingAction.Operation == PendingAction.OperationEnum.Update ||
                pendingAction.Operation == PendingAction.OperationEnum.Delete ||
                pendingAction.Operation == PendingAction.OperationEnum.GenerateIdAndAdd) pendingAction.Model!.BeforeSave(pendingAction.Operation);
        }

        OptimizePendingActions();          
        await ValidatePendingActionsAsync();

        await SaveChangesInternalAsync(PendingActions);

        if (!isFinal)
        {
            //Update all managed models with the new hash and clear PendingAction, so that we can save multiple times in the same unit of work
            foreach (var managedModel in ManagedModels) managedModel.UpdateHash();
            PendingActions.Clear();
        }
    }
    public abstract Task SaveChangesInternalAsync(List<PendingAction> pendingActions);
    #endregion

    #region Private Helpers
    protected void PrepareForThrowingException()
    {
        foreach (var pendingAction in PendingActions.Where(x => x.Operation == PendingAction.OperationEnum.GenerateIdAndAdd)) pendingAction.Model!.Id = 0;
        MakeCompletedAndFinalized();
    }
    protected void ThrowSupermodelValidationException(SupermodelDataContextValidationException.ValidationError validationError)
    {
        PrepareForThrowingException();
        throw new SupermodelDataContextValidationException(validationError);
    }
    protected void ThrowSupermodelValidationException(List<SupermodelDataContextValidationException.ValidationError> validationErrors)
    {
        PrepareForThrowingException();
        throw new SupermodelDataContextValidationException(validationErrors);
    }
    public virtual async Task ValidatePendingActionsAsync()
    {
        //Validate that all disabled PendingActions have been cleared
        if (PendingActions.Any(x => x.Disabled)) throw new SupermodelException("ValidatePendingActions: should not have any Disabled pending actions");

        //Validate that every Pending Action is valid
        if (PendingActions.Any(x => !x.IsValid())) throw new SupermodelException("One of the PendingActions is invalid. This should never happen");
            
        //Validate no duplicate addorupdate, updates, and deletes. For every Id and type there should only be one Update or Delete or AddOrUpdate
        if (PendingActions.Where(x => x.Operation == PendingAction.OperationEnum.Update || x.Operation == PendingAction.OperationEnum.Delete || x.Operation == PendingAction.OperationEnum.AddOrUpdate).GroupBy(x => new { x.ModelType, x.ModelId }).Any(x => x.Count() > 1))
        {
            throw new SupermodelException("Duplicate addorupdates/updates/deletes in PendingActions");
        }

        //Validate objects themselves
        var validationErrors = new List<SupermodelDataContextValidationException.ValidationError>();
        foreach (var pendingAction in PendingActions.Where(x => x.Model != null))
        {
            var vr = new ValidationResultList();
            if (!await AsyncValidator.TryValidateObjectAsync(pendingAction.Model!, new ValidationContext(pendingAction.Model!), vr))
            {
                var validationError = new SupermodelDataContextValidationException.ValidationError(vr, pendingAction, "There are some Model Validation Errors");
                validationErrors.Add(validationError);
            }
        }
        if (validationErrors.Any()) throw new SupermodelDataContextValidationException(validationErrors);
    }
    protected virtual void OptimizePendingActions()
    {
        //Run the optimization, marking redundant operations Disabled
        for (var i = PendingActions.Count - 1; i >= 0; i--)
        {
            var pendingAction = PendingActions[i];
            if (pendingAction.Disabled) continue;
            switch (pendingAction.Operation)
            {
                case PendingAction.OperationEnum.Delete:
                {
                    for (var j = i - 1; j >= 0; j--)
                    {
                        var pendingAction2 = PendingActions[j];
                        if (pendingAction2.Disabled || pendingAction.ModelId != pendingAction2.ModelId || pendingAction.ModelType != pendingAction2.ModelType) continue;
                        switch (pendingAction2.Operation)
                        {
                            case PendingAction.OperationEnum.Delete: pendingAction.Disabled = true; break;
                            case PendingAction.OperationEnum.AddOrUpdate: pendingAction2.Disabled = true; break;
                            case PendingAction.OperationEnum.Update: pendingAction2.Disabled = true; break;
                            //do nothing for all other operations
                        }
                    }
                    break;
                }
                case PendingAction.OperationEnum.AddOrUpdate:
                {
                    for (var j = i - 1; j >= 0; j--)
                    {
                        var pendingAction2 = PendingActions[j];
                        if (pendingAction2.Disabled || pendingAction.ModelId != pendingAction2.ModelId || pendingAction.ModelType != pendingAction2.ModelType) continue;
                        switch (pendingAction2.Operation)
                        {
                            case PendingAction.OperationEnum.Delete: pendingAction.Disabled = true; break;
                            case PendingAction.OperationEnum.AddOrUpdate: pendingAction2.Disabled = true; break;
                            case PendingAction.OperationEnum.Update: pendingAction2.Disabled = true; break;
                            //do nothing for all other operations
                        }
                    }
                    break;
                }
                case PendingAction.OperationEnum.Update:
                {
                    for (var j = i - 1; j >= 0; j--)
                    {
                        var pendingAction2 = PendingActions[j];
                        if (pendingAction2.Disabled || pendingAction.ModelId != pendingAction2.ModelId || pendingAction.ModelType != pendingAction2.ModelType) continue;
                        switch (pendingAction2.Operation)
                        {
                            case PendingAction.OperationEnum.Delete: pendingAction.Disabled = true; break;
                            case PendingAction.OperationEnum.AddOrUpdate: pendingAction2.Disabled = true; pendingAction.Operation = PendingAction.OperationEnum.AddOrUpdate; break;
                            case PendingAction.OperationEnum.Update: pendingAction2.Disabled = true; break;
                            //do nothing for all other operations
                        }
                    }
                    break;
                }
                //do nothing for all other operations
            }
        }
            
        //Remove disabled operations
        PendingActions.RemoveAll(x => x.Disabled);
    }
    #endregion

    #region Properties & Constants
    protected List<ManagedModel> ManagedModels { get; set; }
    protected List<PendingAction> PendingActions { get; set; }
    public Dictionary<string, object> CustomValues { get; } 
    #endregion
}