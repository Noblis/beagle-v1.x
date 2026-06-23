using System.Text;
using SQLite;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.DataContext.Sqlite;
using Supermodel.Client.Backend.DataContext.WebApi;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.Repository;
using Supermodel.Client.Backend.UnitOfWork;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.Encryptor;
using Supermodel.ReflectionMapper;

namespace Supermodel.Client.Backend.DataContext.CachedWebApi;

public class CachedWebApiDataContext<TWebApiDataContext, TSqliteDataContext> : DataContextBase, IWebApiAuthorizationContext, ICachedDataContext
    where TWebApiDataContext : WebApiDataContext, new()
    where TSqliteDataContext : SqliteDataContext, new()
{
    #region Constructors
    public CachedWebApiDataContext()
    {
        //if (Pick.RunningPlatform() == Platform.DotNetCore) throw new SupermodelException("Supermodel's CachedWebApiDataContext is only supported on mobile platforms");
        CacheAgeToleranceInSeconds = 5 * 60; // 5 min
    }
    #endregion

    #region Methods
    public async Task PurgeCacheAsync(int? cacheExpirationAgeInSeconds = null, Type? modelType = null)
    {
        var sqliteContext = new TSqliteDataContext();
        await sqliteContext.InitDbAsync();
        var db = new SQLiteAsyncConnection(sqliteContext.DatabaseFilePath);
        var sb = new StringBuilder();
        sb.AppendFormat(@"DELETE FROM {0}", sqliteContext.DataTableName);
        var first = true;
                    
        if (cacheExpirationAgeInSeconds != null)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (first)
            {
                sb.Append(" WHERE ");
                first = false;
            }
            else
                // ReSharper disable HeuristicUnreachableCode
            {
                sb.Append(" AND ");
            }
            // ReSharper restore HeuristicUnreachableCode
            sb.AppendFormat("BroughtFromMasterDbOnUtcTicks < {0}", DateTime.UtcNow.AddSeconds(cacheExpirationAgeInSeconds.Value).Ticks);
        }

        if (modelType != null)
        {
            if (first)
            {
                sb.Append(" WHERE ");
                first = false;
            }
            else
            {
                sb.Append(" AND ");
            }
            sb.AppendFormat("ModelTypeLogicalName == '{0}'", GetModelTypeLogicalName(modelType));
        }

        if (first)
        {
            sb.Append(" WHERE ");
            // ReSharper disable once RedundantAssignment
            first = false;
        }
        else
        {
            sb.Append(" AND ");
        }
        sb.AppendFormat("ModelTypeLogicalName != '{0}'", sqliteContext.SchemaVersionModelType);

        var commandText = sb.ToString();
        await db.ExecuteAsync(commandText);
    }
    #endregion

    #region ValidateLogin
    public virtual async Task<LoginResult> ValidateLoginAsync<TModel>() where TModel : class, IModel
    {
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;

            return await UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.ValidateLoginAsync<TModel>();
        }
    }
    #endregion

    #region DataContext Reads
    public override async Task<TModel?> GetByIdOrDefaultAsync<TModel>(long id) where TModel : class
    {
        //First check local cache
        await using (new UnitOfWork<TSqliteDataContext>())
        {
            var cachedModel = await RepoFactory.Create<TModel>().GetByIdOrDefaultAsync(id);
            if (cachedModel != null)
            {
                if (cachedModel.BroughtFromMasterDbOnUtc != null && cachedModel.BroughtFromMasterDbOnUtc.Value.AddSeconds(CacheAgeToleranceInSeconds) > DateTime.UtcNow)
                {
                    ManagedModels.Add(new ManagedModel(cachedModel));
                    return cachedModel;
                }
                else
                {
                    cachedModel.Delete();
                }
                await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.FinalSaveChangesAsync();
            }
            else
            {
                //Mark done for performance reasons
                UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.MakeCompletedAndFinalized();
            }
        }

        //if we get here, we need to get the data from web api service
        TModel? masterModel;
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;
                
            masterModel = await RepoFactory.Create<TModel>().GetByIdOrDefaultAsync(id);
        }

        //Now save master model to cache and add it to managed models if we have something to save
        if (masterModel != null)
        {
            await using (new UnitOfWork<TSqliteDataContext>())
            {
                ManagedModels.Add(new ManagedModel(masterModel)); 
                UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.AddOrUpdate(masterModel);
                //await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.FinalSaveChangesAsync();
            }
                
        }

        return masterModel;
    }
    public override async Task<List<TModel>> GetAllAsync<TModel>(int? skip = null, int? take = null)
    {
        //first delete local cache, we will be refreshing it with new data
        var sqliteContext = new TSqliteDataContext();
        await sqliteContext.InitDbAsync();
        var db = new SQLiteAsyncConnection(sqliteContext.DatabaseFilePath);
        var commandText = $@"DELETE FROM {sqliteContext.DataTableName} WHERE ModelTypeLogicalName == '{GetModelTypeLogicalName(typeof(TModel))}'";
        await db.ExecuteAsync(commandText);
            
        //get the data from web api service
        List<TModel> masterModels;
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;

            masterModels = await RepoFactory.Create<TModel>().GetAllAsync(skip, take);
        }

        //Now save master models to cache if we have something to save
        if (masterModels.Any())
        {
            await using (new UnitOfWork<TSqliteDataContext>())
            {
                foreach (var masterModel in masterModels)
                {
                    ManagedModels.Add(new ManagedModel(masterModel)); 
                    UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.AddOrUpdate(masterModel);
                }
                //await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.FinalSaveChangesAsync();
            }
        }

        return masterModels;
    }
    public override async Task<long> GetCountAllAsync<TModel>(int? skip = null, int? take = null)
    {
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;

            return await UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.GetCountAllAsync<TModel>(skip, take);
        }
    }
    #endregion

    #region DataContext Queries
    public override async Task<List<TModel>> GetWhereAsync<TModel>(object? searchBy, string? sortBy = null, int? skip = null, int? take = null)
    {
        //if we get here, we need to get the data from web api service
        List<TModel> masterModels;
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;

            masterModels = await RepoFactory.Create<TModel>().GetWhereAsync(searchBy, sortBy, skip, take);
        }

        //Now save master models to cache if we have something to save
        if (masterModels.Any())
        {
            await using (new UnitOfWork<TSqliteDataContext>())
            {
                foreach (var masterModel in masterModels)
                {
                    ManagedModels.Add(new ManagedModel(masterModel));
                    UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.AddOrUpdate(masterModel);
                }
                //await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.FinalSaveChangesAsync();
            }
        }

        return masterModels;
    }
    public override async Task<long> GetCountWhereAsync<TModel>(object? searchBy, int? skip = null, int? take = null)
    {
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;

            return await UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.GetCountWhereAsync<TModel>(searchBy, skip, take);
        }
    }
    #endregion

    #region DataContext Save Changes
    public override async Task SaveChangesInternalAsync(List<PendingAction> pendingActions)
    {
        //First we delete all objects in cache that are about to be updated -- we do this for transactional integrity
        var actionsToLoopThrough = IsReadOnly ? PendingActions.Where(x => x.IsReadOnlyAction) : PendingActions;
        await using (new UnitOfWork<TSqliteDataContext>())
        {
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var pendingAction in actionsToLoopThrough)
            {
                switch (pendingAction.Operation)
                {
                    case PendingAction.OperationEnum.AddWithExistingId:
                    case PendingAction.OperationEnum.Update:
                    case PendingAction.OperationEnum.Delete:
                    case PendingAction.OperationEnum.AddOrUpdate:
                    case PendingAction.OperationEnum.DelayedGetById:
                    case PendingAction.OperationEnum.DelayedGetByIdOrDefault:
                    {
                        UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.ExecuteGenericMethod("Delete", [pendingAction.ModelType!], pendingAction.ModelId);
                        break;
                    }
                    case PendingAction.OperationEnum.DelayedGetAll:
                    {
                        var sqliteContext = new TSqliteDataContext();
                        await sqliteContext.InitDbAsync();
                        var db = new SQLiteAsyncConnection(sqliteContext.DatabaseFilePath);
                        var commandText = $@"DELETE FROM {sqliteContext.DataTableName} WHERE ModelTypeLogicalName == '{GetModelTypeLogicalName(pendingAction.ModelType)}'";
                        await db.ExecuteAsync(commandText);
                        break;
                    }
                    case PendingAction.OperationEnum.GenerateIdAndAdd:
                    case PendingAction.OperationEnum.DelayedGetWhere:
                    case PendingAction.OperationEnum.DelayedGetCountAll:
                    case PendingAction.OperationEnum.DelayedGetCountWhere:
                    {
                        //for these we do nothing
                        break;
                    }
                    default:
                    {
                        throw new SupermodelException("Unsupported Operation");
                    }
                }
            }
            //await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.FinalSaveChangesAsync();
        }

        //Then we attempt to save to web api service
        await using (new UnitOfWork<TWebApiDataContext>())
        {
            if (IsReadOnly) UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeReadOnly();
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.AuthHeader = AuthHeader;

            await UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.SaveChangesInternalAsync(PendingActions);
            UnitOfWorkContext<TWebApiDataContext>.CurrentDataContext.MakeCompletedAndFinalized();
        }

        //Them if we were successful, we update local db
        await using (new UnitOfWork<TSqliteDataContext>())
        {
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var pendingAction in actionsToLoopThrough)
            {
                switch (pendingAction.Operation)
                {
                    case PendingAction.OperationEnum.AddWithExistingId:
                    case PendingAction.OperationEnum.GenerateIdAndAdd:
                    case PendingAction.OperationEnum.Update:
                    case PendingAction.OperationEnum.AddOrUpdate:
                    {
                        UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.ExecuteGenericMethod("AddOrUpdate", [pendingAction.ModelType!], pendingAction.Model);
                        break;
                    }
                    case PendingAction.OperationEnum.Delete: //we need to delete again because we could have read data with delayed read
                    {
                        UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.ExecuteGenericMethod("Delete", [pendingAction.ModelType!], pendingAction.ModelId);
                        break;
                    }
                    case PendingAction.OperationEnum.DelayedGetById:
                    case PendingAction.OperationEnum.DelayedGetByIdOrDefault:
                    {
                        var model = (IModel?)pendingAction.DelayedValue!.GetValue();
                        if (model != null)
                        {
                            UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.ExecuteGenericMethod("AddOrUpdate", [pendingAction.ModelType!], model);
                        }
                        break;
                    }
                    case PendingAction.OperationEnum.DelayedGetAll:
                    case PendingAction.OperationEnum.DelayedGetWhere:
                    {
                        var models = (IEnumerable<IModel>)pendingAction.DelayedValue!.GetValue()!;
                        foreach (var model in models)
                        {
                            UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.ExecuteGenericMethod("AddOrUpdate", [pendingAction.ModelType!], model);
                        }
                        break;
                    }
                    case PendingAction.OperationEnum.DelayedGetCountAll:
                    case PendingAction.OperationEnum.DelayedGetCountWhere:
                    {
                        //for these we do nothing
                        break;
                    }
                    default:
                    {
                        throw new SupermodelException("Unsupported Operation");
                    }
                }
            }
            //await UnitOfWorkContext<TSqliteDataContext>.CurrentDataContext.FinalSaveChangesAsync();
        }
    }
    #endregion

    #region Configuration Properties
    public AuthHeader? AuthHeader { get; set; }
    public int CacheAgeToleranceInSeconds { get; set; }
    #endregion
}