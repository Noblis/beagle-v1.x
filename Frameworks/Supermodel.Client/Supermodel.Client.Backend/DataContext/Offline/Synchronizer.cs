using SQLite;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.DataContext.Sqlite;
using Supermodel.Client.Backend.DataContext.WebApi;
using Supermodel.Client.Backend.Exceptions;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.Repository;
using Supermodel.Client.Backend.UnitOfWork;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.ReflectionMapper;

namespace Supermodel.Client.Backend.DataContext.Offline;

public abstract class Synchronizer<TModel, TWebApiDataContext, TSqliteDataContext> 
    where TModel : class, IModel, new()
    where TSqliteDataContext : SqliteDataContext, new()
    where TWebApiDataContext : WebApiDataContext, new()
{
    #region Constructiors
    protected Synchronizer()
    {
        RefreshFromMasterAfterSync = true;
    }
    #endregion

    #region Methods
    public async Task SynchronizeAsync()
    {
        await using(var webApiUOW = new UnitOfWork<TWebApiDataContext>())
        {
            SetUpWebApiContext(UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext);

            await using(var sqliteUOW = new UnitOfWork<TSqliteDataContext>())
            {
                SetUpSqliteContext(UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext);
                    
                //----------------------------------------------------------------------------------------
                //Load all the models to synchronize
                //----------------------------------------------------------------------------------------
                var masterModels = await LoadAllSyncFromMasterAsync();
                var localModels = await LoadAllSyncFromLocalAsync();
                    
                //----------------------------------------------------------------------------------------
                //Run the syncing algorithm
                //----------------------------------------------------------------------------------------
                await SyncListsAsync(masterModels, localModels);

                //----------------------------------------------------------------------------------------
                //Let's try to validate all the models that are to be saved locally
                //----------------------------------------------------------------------------------------
                SupermodelDataContextValidationException? localValidationException = null;
                try
                {
                    await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.ValidatePendingActionsAsync();
                }
                catch (SupermodelDataContextValidationException ex)
                {
                    localValidationException = ex;
                }
                catch (Exception)
                {
                    webApiUOW.Context.CommitOnDispose = sqliteUOW.Context.CommitOnDispose = false;
                    throw;
                }
                if (localValidationException != null)
                {
                    await ResolveLocalValidationError(localValidationException, webApiUOW, sqliteUOW);
                }

                //----------------------------------------------------------------------------------------
                //Register for refresh if needed
                //----------------------------------------------------------------------------------------

                DelayedModels<TModel>? delayedMasterModels = null;
                if (RefreshFromMasterAfterSync)
                {
                    var sqliteDataContext = UnitOfWorkContext<TSqliteDataContext>.PopDbContext();
                    UnitOfWorkContext.DetectUpdates();
                    UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteDataContext);
                    RegisterDelayedLoadAllSyncFromMaster(out delayedMasterModels);
                }

                //----------------------------------------------------------------------------------------
                //Then try to save changes to the web api
                //----------------------------------------------------------------------------------------
                UnitOfWorkContext<TSqliteDataContext>.PopDbContext();

                SupermodelDataContextValidationException? serverValidationException = null;
                try
                {
                    await UnitOfWorkContext.FinalSaveChangesAsync();
                }
                catch (SupermodelDataContextValidationException ex)
                {
                    serverValidationException = ex;
                }
                catch(Exception)
                {
                    webApiUOW.Context.CommitOnDispose = sqliteUOW.Context.CommitOnDispose = false;
                    throw;
                }
                    
                if (serverValidationException != null)
                {
                    await ResolveServerValidationError(serverValidationException, webApiUOW, sqliteUOW);
                }
                UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteUOW.Context);

                //----------------------------------------------------------------------------------------
                //Refresh local models if we have data to refresh with (that is if we registered for it earlier)
                //----------------------------------------------------------------------------------------
                if (delayedMasterModels != null)
                {
                    foreach (var localModel in localModels)
                    {
                        var matchingDelayedMasterModel = delayedMasterModels.Values!.SingleOrDefault(x => x.Id == localModel.Id);
                        if (matchingDelayedMasterModel != null) await CopyModel1IntoModel2Async(matchingDelayedMasterModel, localModel);
                    }
                }

                //----------------------------------------------------------------------------------------
                //If that succeeds, then we save changes to the local db, should not have any more validation errors, since we already checked
                //----------------------------------------------------------------------------------------
                await SetLastSyncDateTimeUtcAsync(DateTime.UtcNow);
                //await UnitOfWorkContext.FinalSaveChangesAsync();
            }
        }
    }
    public async Task<bool> IsUploadPendingAsync(TModel model)
    {
        var lastSyncDateTimeUtc = await GetLastSyncDateTimeUtcAsync();
        return lastSyncDateTimeUtc == null || GetModifiedDateTimeUtc(model) > lastSyncDateTimeUtc;
    }
    #endregion

    #region Main Algorithm
    protected virtual async Task SyncListsAsync(List<TModel> masterModels, List<TModel> localModels)
    {
        //find updated on the server, update locally
        //find updated on the client, update on the server
        //find created on the server, create locally
        //find deleted locally, delete on the server
        foreach (var masterModel in masterModels)
        {
            //Try to find a matching local model
            var matchingLocalModel = localModels.SingleOrDefault(x => x.Id == masterModel.Id);
            if (matchingLocalModel != null) //if we found one
            {
                var lastSyncDateTimeUtc = await GetLastSyncDateTimeUtcAsync();
                if (GetModifiedDateTimeUtc(masterModel) > lastSyncDateTimeUtc && GetModifiedDateTimeUtc(matchingLocalModel) > lastSyncDateTimeUtc)
                {
                    //if model was updated on both server and client, call the hook to let the user resolve conflict
                    await HandleModelUpdatedOnServerAndDeviceAsync(masterModel, matchingLocalModel);
                }
                else
                {
                    //otherwise, we just figure out the newer one and copy it into the older one
                    await CopyNewerModelIntoOlderAsync(masterModel, matchingLocalModel);
                }
            }
            else //if not, there could be two scenarios:
            {
                //If the model on the server was created after our last sync or if we never synced before
                var lastSyncDateTimeUtc = await GetLastSyncDateTimeUtcAsync();
                if (GetCreatedDateTimeUtc(masterModel) > lastSyncDateTimeUtc || lastSyncDateTimeUtc == null)
                {
                    //we need to add the master model to our local storage
                    //var localModel = new TModel();
                    //CopyModel1IntoModel2(masterModel, localModel);
                    //localModel.Add();
                    masterModel.Add(); //Add master model to local storage
                }
                else
                {
                    //otherwise it means that we deleted the model on the client and now we need to delete it from the server
                    if (GetModifiedDateTimeUtc(masterModel) > lastSyncDateTimeUtc)
                    {
                        //if since our last sync the model was modified on the server and deleted on the client, call the hook to let the user resolve conflict
                        HandleModelUpdatedOnServerDeletedOnDevice(masterModel);
                    }
                    else
                    {
                        var sqliteDataContext = UnitOfWorkContext<TSqliteDataContext>.PopDbContext();
                        masterModel.Delete();
                        UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteDataContext);
                    }
                }
            }
        }

        //find created locally, create on the server
        //find deleted on the server, delete locally
        foreach (var localModel in localModels)
        {
            if (localModel.Id < 0)
            {
                //if this model was never created on the server, go ahead and create it
                var sqliteDataContext = UnitOfWorkContext<TSqliteDataContext>.PopDbContext();
                localModel.Id = 0; //make sure when we add it on the server, Id == 0. When it is saved on the server, the local model's id should get updated
                localModel.Add(); //add local model to server context
                UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteDataContext);
            }
            else
            {
                //Try to find a matching server model
                var matchingServerModel = masterModels.SingleOrDefault(x => x.Id == localModel.Id);
                if (matchingServerModel == null) //if one exists, we already updated it and need not worry
                {
                    //otherwise, it means that the model was deleted on the server
                    var lastSyncDateTimeUtc = await GetLastSyncDateTimeUtcAsync();
                    if (GetModifiedDateTimeUtc(localModel) > lastSyncDateTimeUtc)
                    {
                        //if since our last synch the model was modified on the device and deleted on the client, call the hook to let the user resolve conflict
                        HandleModelUpdatedOnDeviceDeletedOnServer(localModel);
                    }
                    else
                    {
                        localModel.Delete();
                    }
                }
            }
        }
    }
    #endregion

    #region Conflict Resolution Methods
    protected virtual Task ResolveLocalValidationError(SupermodelDataContextValidationException validationException, UnitOfWork<TWebApiDataContext> webApiUOW, UnitOfWork<TSqliteDataContext> sqliteUOW)
    {
        //default implementation just rolls back transactions and rethrows the exception
        webApiUOW.Context.CommitOnDispose = sqliteUOW.Context.CommitOnDispose = false;
        throw validationException;
    }
    protected virtual Task ResolveServerValidationError(SupermodelDataContextValidationException validationException, UnitOfWork<TWebApiDataContext> webApiUOW, UnitOfWork<TSqliteDataContext> sqliteUOW)
    {
        //default implementation just rolls back transactions and rethrows the exception
        webApiUOW.Context.CommitOnDispose = sqliteUOW.Context.CommitOnDispose = false;
        throw validationException;
    }
    protected virtual void HandleModelUpdatedOnDeviceDeletedOnServer(TModel localModel)
    {
        //default implements "last one wins" approach
        localModel.Delete();
    }
    protected virtual Task HandleModelUpdatedOnServerAndDeviceAsync(TModel masterModel, TModel localModel)
    {
        //default implements "last one wins" approach
        return CopyNewerModelIntoOlderAsync(masterModel, localModel);
    }
    protected virtual void HandleModelUpdatedOnServerDeletedOnDevice(TModel masterModel)
    {
        //default implements "last one wins" approach
        var sqliteDataContext = UnitOfWorkContext<TSqliteDataContext>.PopDbContext();
        masterModel.Delete();
        UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteDataContext);
    }
    #endregion

    #region Modified and Created Utc DateTime Resolution
    public abstract DateTime GetModifiedDateTimeUtc(TModel model);
    public abstract DateTime GetCreatedDateTimeUtc(TModel model);
    #endregion

    #region Helpers that are Meant to be Overriden for Customization
    protected virtual void RegisterDelayedLoadAllSyncFromMaster(out DelayedModels<TModel> delayedMasterModels)
    {
        var sqliteDataContext = UnitOfWorkContext<TSqliteDataContext>.PopDbContext();
        RepoFactory.Create<TModel>().DelayedGetAll(out delayedMasterModels);
        UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteDataContext);
    }
    protected virtual Task<List<TModel>> LoadAllSyncFromMasterAsync()
    {
        var sqliteDataContext = UnitOfWorkContext<TSqliteDataContext>.PopDbContext();
        var result = RepoFactory.Create<TModel>().GetAllAsync();
        UnitOfWorkContext<TSqliteDataContext>.PushDbContext(sqliteDataContext);
        return result;
    }
    protected virtual Task<List<TModel>> LoadAllSyncFromLocalAsync()
    {
        return RepoFactory.Create<TModel>().GetAllAsync();
    }
    public virtual async Task CopyModel1IntoModel2Async(TModel model1, TModel model2)
    {
        if (model1.BroughtFromMasterDbOnUtc == null || model2.BroughtFromMasterDbOnUtc == null) throw new SupermodelException("(model1.BroughtFromMasterDbOnUtc == null || model2.BroughtFromMasterDbOnUtc == null): this should not happen");
        model1.BroughtFromMasterDbOnUtc = model2.BroughtFromMasterDbOnUtc = (model1.BroughtFromMasterDbOnUtc > model2.BroughtFromMasterDbOnUtc ? model1.BroughtFromMasterDbOnUtc : model2.BroughtFromMasterDbOnUtc);
        await model2.MapFromAsync(model1, true); //note that we force shallow copy here for performance reasons
    }
    public virtual async Task CopyNewerModelIntoOlderAsync(TModel model1, TModel model2)
    {
        if (GetModifiedDateTimeUtc(model1) > GetModifiedDateTimeUtc(model2)) await CopyModel1IntoModel2Async(model1, model2);
        else await CopyModel1IntoModel2Async(model2, model1);
    }
    protected abstract void SetUpWebApiContext(TWebApiDataContext context);
    protected abstract void SetUpSqliteContext(TSqliteDataContext context);
    #endregion

    #region LastSync DateTime Handling
    public virtual async Task<DateTime?> GetLastSyncDateTimeUtcAsync()
    {
        return _lastSyncDateTimeUtc ??= await GetLastSyncDateTimeUtcInternalAsync();
    }
    public virtual async Task SetLastSyncDateTimeUtcAsync(DateTime value)
    {
        _lastSyncDateTimeUtc = value;
        await SetLastSyncDateTimeUtcInternalAsync(value);
    }
    private DateTime? _lastSyncDateTimeUtc;
        
    protected virtual async Task<DateTime?> GetLastSyncDateTimeUtcInternalAsync()
    {
        var sqLiteDbContext = new TSqliteDataContext();
        var db = new SQLiteAsyncConnection(sqLiteDbContext.DatabaseFilePath);
        var commandText = $"SELECT * FROM [{sqLiteDbContext.DataTableName}] WHERE ModelTypeLogicalName = '{sqLiteDbContext.DataTableName + SchemaVersionModelTypeSuffix}'";
        var results = await db.QueryAsync<DataRow>(commandText);
        if (results.Count == 0) return null;
        if (results.Count > 1) return null;
        var lastSyncDateTimeUtc = DateTime.Parse(results.Single().Json!);
        return lastSyncDateTimeUtc;
    }
    protected virtual async Task SetLastSyncDateTimeUtcInternalAsync(DateTime value)
    {
        var sqLiteDbContext = new TSqliteDataContext();
        var db = new SQLiteAsyncConnection(sqLiteDbContext.DatabaseFilePath);
        string commandText;
        if (await GetLastSyncDateTimeUtcAsync() == null)
        {
            //insert
            commandText = 
                $@"INSERT INTO [{sqLiteDbContext.DataTableName}] (
                    ModelTypeLogicalName, 
                    ModelId, 
                    Json, 
                    BroughtFromMasterDbOnUtcTicks, 
                    Index0, 
                    Index1, 
                    Index2, 
                    Index3, 
                    Index4,
                    Index5, 
                    Index6, 
                    Index7, 
                    Index8, 
                    Index9,
                    Index10, 
                    Index11, 
                    Index12, 
                    Index13, 
                    Index14,
                    Index15, 
                    Index16, 
                    Index17, 
                    Index18, 
                    Index19,
                    Index20, 
                    Index21, 
                    Index22, 
                    Index23, 
                    Index24,
                    Index25, 
                    Index26, 
                    Index27, 
                    Index28, 
                    Index29) 
			        VALUES (
                    '{sqLiteDbContext.DataTableName + SchemaVersionModelTypeSuffix}', 
                    0, 
                    '{value}', 
                    {DateTime.UtcNow.Ticks}, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL,
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL,
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL,
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL,
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL,
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL, 
                    NULL)";
        }
        else
        {
            //update
            commandText = 
                $@"UPDATE [{sqLiteDbContext.DataTableName}]
                SET 
                    Json = '{value}', 
                    BroughtFromMasterDbOnUtcTicks = {DateTime.UtcNow.Ticks},
                    Index0 = NULL, 
                    Index1 = NULL, 
                    Index2 = NULL, 
                    Index3 = NULL, 
                    Index4 = NULL,
                    Index5 = NULL, 
                    Index6 = NULL, 
                    Index7 = NULL, 
                    Index8 = NULL, 
                    Index9 = NULL,
                    Index10 = NULL, 
                    Index11 = NULL, 
                    Index12 = NULL, 
                    Index13 = NULL, 
                    Index14 = NULL,
                    Index15 = NULL, 
                    Index16 = NULL, 
                    Index17 = NULL, 
                    Index18 = NULL, 
                    Index19 = NULL,
                    Index20 = NULL, 
                    Index21 = NULL, 
                    Index22 = NULL, 
                    Index23 = NULL, 
                    Index24 = NULL,
                    Index25 = NULL, 
                    Index26 = NULL, 
                    Index27 = NULL, 
                    Index28 = NULL, 
                    Index29 = NULL
                WHERE ModelTypeLogicalName = '{sqLiteDbContext.DataTableName + SchemaVersionModelTypeSuffix}' AND ModelId = 0";
        }
        await db.ExecuteAsync(commandText);
    }
    public string SchemaVersionModelTypeSuffix => ".LocalDb.lastSyncDateTimeUtc";
    #endregion

    #region Properties
    public bool RefreshFromMasterAfterSync { get; set; }
    #endregion
}