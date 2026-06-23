using SQLite;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.UnitOfWork;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.LogicalContext;
using System.Text;

namespace Supermodel.Client.Backend.DataContext.Sqlite;

public abstract class SqliteDataContext : DataContextBase, ISqlQueryProvider
{
    #region ISqlQueryProvider implemetation
    // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
    public virtual object? GetIndex<TModel>(int idxNum0To29, TModel model)
    {
        if (idxNum0To29 < 0 || idxNum0To29 > 29) throw new SupermodelException("Only Indexes 0-29 are allowed");
        // ReSharper disable ConditionIsAlwaysTrueOrFalse
        if (idxNum0To29 >= 0 && idxNum0To29 <= 9) return GetStringIndex(idxNum0To29, model);
        if (idxNum0To29 >= 10 && idxNum0To29 <= 19) return GetLongIndex(idxNum0To29 - 10, model);
        if (idxNum0To29 >= 20 && idxNum0To29 <= 29) return GetDoubleIndex(idxNum0To29 - 20, model);
        // ReSharper restore ConditionIsAlwaysTrueOrFalse
        return null;
    }
    public virtual string? GetStringIndex<TModel>(int idxNum0To9, TModel model)
    {
        if (idxNum0To9 < 0 || idxNum0To9 > 9) throw new SupermodelException("Only Indexes 0-9 are allowed");
        return null;
    }
    public virtual long? GetLongIndex<TModel>(int idxNum0To9, TModel model)
    {
        if (idxNum0To9 < 0 || idxNum0To9 > 9) throw new SupermodelException("Only Indexes 0-9 are allowed");
        return null;
    }
    public virtual double? GetDoubleIndex<TModel>(int idxNum0To9, TModel model)
    {
        if (idxNum0To9 < 0 || idxNum0To9 > 9) throw new SupermodelException("Only Indexes 0-9 are allowed");
        return null;
    }
    public virtual string? GetWhereClause<TModel>(object? searchBy, string? sortBy)
    {
        return null;
    }
    public virtual string GetSkipAndTakeForWhereClause<TModel>(int? skip, int? take)
    {
        var sb = new StringBuilder();
        if (take != null) sb.Append(" LIMIT " + take);
        if (skip != null) sb.Append(" OFFSET " + skip);
        return sb.ToString();
    }
    // ReSharper restore ParameterOnlyUsedForPreconditionCheck.Global
    #endregion

    #region DataContext Reads
    public override async Task<TModel?> GetByIdOrDefaultAsync<TModel>(long id) where TModel : class
    {
        if (await InitDbAsync()) return null;

        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var modelTypeLogicalName = GetModelTypeLogicalName(typeof(TModel));
        var commandText = $"SELECT * FROM [{DataTableName}] WHERE ModelTypeLogicalName = '{modelTypeLogicalName}' AND ModelId = {id}";
        var results = await db.QueryAsync<DataRow<TModel>>(commandText);
        if (results.Count == 0) return null;
        if (results.Count > 1) throw new Exception("GetByIdOrDefaultAsync brought back more than one record");
        var model = results.Single().GetModel();
        ManagedModels.Add(new ManagedModel(model));
        return model;
    }
    public override async Task<List<TModel>> GetAllAsync<TModel>(int? skip = null, int? take = null)
    {
        if (await InitDbAsync()) return new List<TModel>();

        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var modelTypeLogicalName = GetModelTypeLogicalName(typeof(TModel));
        var commandText = $"SELECT * FROM [{DataTableName}] WHERE ModelTypeLogicalName = '{modelTypeLogicalName}'";
        if (take != null) commandText += " LIMIT " + take;
        if (skip != null) commandText += " OFFSET " + skip;
        var results = await db.QueryAsync<DataRow<TModel>>(commandText);
        var models = new List<TModel>();
        foreach (var result in results)
        {
            var model = result.GetModel();
            ManagedModels.Add(new ManagedModel(model));
            models.Add(model);
        }
        return models;
    }
    public override async Task<long> GetCountAllAsync<TModel>(int? skip = null, int? take = null)
    {
        if (await InitDbAsync()) return 0;

        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var modelTypeLogicalName = GetModelTypeLogicalName(typeof(TModel));
        var commandText = $"SELECT COUNT(*) FROM [{DataTableName}] WHERE ModelTypeLogicalName='{modelTypeLogicalName}'";
        if (take != null) commandText += " LIMIT " + take;
        if (skip != null) commandText += " OFFSET " + skip;
        var count = await db.ExecuteScalarAsync<long>(commandText);
        return count;
    }
    #endregion

    #region DataContext Queries
    public override async Task<List<TModel>> GetWhereAsync<TModel>(object? searchBy, string? sortBy = null, int? skip = null, int? take = null)
    {
        if (await InitDbAsync()) return new List<TModel>();

        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var modelTypeLogicalName = GetModelTypeLogicalName(typeof(TModel));
        var whereClause = GetWhereClause<TModel>(searchBy, sortBy);
        if (whereClause == null) throw new SupermodelException("Must override GetWhereClause before running queries on localDb");
        var fullWhereClause = whereClause + GetSkipAndTakeForWhereClause<TModel>(skip, take);
        var commandText = $"SELECT * FROM [{DataTableName}] WHERE ModelTypeLogicalName = '{modelTypeLogicalName}' {fullWhereClause}";

        var results = await db.QueryAsync<DataRow<TModel>>(commandText);
        var models = new List<TModel>();
        foreach (var result in results)
        {
            var model = result.GetModel();
            ManagedModels.Add(new ManagedModel(model));
            models.Add(model);
        }
        return models;
    }
    public override async Task<long> GetCountWhereAsync<TModel>(object? searchBy, int? skip = null, int? take = null)
    {
        if (await InitDbAsync()) return 0;

        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var modelTypeLogicalName = GetModelTypeLogicalName(typeof(TModel));
        var whereClause = GetWhereClause<TModel>(searchBy, null);
        if (whereClause == null) throw new SupermodelException("Must override GetWhereClause before running queries on localDb");
        var fullWhereClause = whereClause + GetSkipAndTakeForWhereClause<TModel>(skip, take);
        var commandText = $"SELECT COUNT(*) FROM [{DataTableName}] WHERE ModelTypeLogicalName = '{modelTypeLogicalName}' {fullWhereClause}";
        var count = await db.ExecuteScalarAsync<long>(commandText);
        return count;
    }
    #endregion

    #region DataContext sqlite-specific writes
    public void AddOrUpdate<TModel>(TModel model) where TModel : class, IModel, new()
    {
        if (model.Id == 0) throw new SupermodelException("AddOrUpdate operation does not allow model id to be 0");
        PendingActions.Add(new PendingAction
        {
            Operation = PendingAction.OperationEnum.AddOrUpdate,
            ModelType = typeof(TModel),
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
    #endregion

    #region DataContext Save Changes
    protected override async Task SaveChangesAsync(bool isFinal)
    {
        await InitDbAsync();
        await base.SaveChangesAsync(isFinal);
    }
    public override async Task SaveChangesInternalAsync(List<PendingAction> pendingActions)
    {
        await InitDbAsync();

        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        try
        {
            await db.RunInTransactionAsync(transaction => 
            {
                for (var i = 0; i < pendingActions.Count; i++)
                {
                    var pendingAction = pendingActions[i];
                        
                    //if we have multiple deletes in a row
                    if (pendingAction.Operation == PendingAction.OperationEnum.Delete && i + 1 < pendingActions.Count && pendingActions[i + 1].Operation == PendingAction.OperationEnum.Delete)
                    {
                        var sb = new StringBuilder();
                        sb.Append(pendingAction.GenerateSql(DataTableName, this));
                            
                        int j;
                        var brokeFromLoop = false;
                        for (j = 1; i + j < pendingActions.Count; j++)
                        {
                            var pendingActionIPlusJ = pendingActions[i + j];
                            if (pendingActionIPlusJ.Operation != PendingAction.OperationEnum.Delete)
                            {
                                brokeFromLoop = true;
                                break;
                            }
                            sb.AppendFormat(@" OR ModelTypeLogicalName = '{0}' AND ModelId = {1}", GetModelTypeLogicalName(pendingActionIPlusJ.ModelType), pendingActionIPlusJ.ModelId);
                        }

                        var bulkDeleteCommandText = sb.ToString();
                        transaction.Execute(bulkDeleteCommandText);

                        if (brokeFromLoop) i = i + j - 1;
                        else i = i + j;
                            
                        continue;
                    }

                    //if we generate Id, we need to read the nextId first
                    if (pendingAction.Operation == PendingAction.OperationEnum.GenerateIdAndAdd)
                    {
                        // ReSharper disable once StringLiteralTypo
                        var nextIdCommandText = $"SELECT IFNULL(MIN(ModelId), 0)-1 FROM {DataTableName} WHERE ModelId < 0 AND ModelTypeLogicalName='{GetModelTypeLogicalName(pendingAction.ModelType)}'";
                        var nextId = transaction.ExecuteScalar<long>(nextIdCommandText);
                        pendingAction.Model!.Id = nextId;
                    }

                    var commandText = pendingAction.GenerateSql(DataTableName, this);
                    switch (pendingAction.SqlType)
                    {
                        case PendingAction.OperationSqlType.NoQueryResult:
                        {
                            transaction.Execute(commandText);
                            pendingAction.ProcessDatabaseResponseAsync(DataTableName, null, this);
                            break;
                        }
                        case PendingAction.OperationSqlType.SingleResultQuery:
                        case PendingAction.OperationSqlType.ListResultQuery:
                        {
                            var results = transaction.Query<DataRow>(commandText);
                            pendingAction.ProcessDatabaseResponseAsync(DataTableName, results, this);
                            break;
                        }
                        case PendingAction.OperationSqlType.ScalarResultQuery:
                        {
                            var scalar = transaction.ExecuteScalar<long>(commandText);
                            pendingAction.ProcessDatabaseResponseAsync(DataTableName, scalar, this);
                            break;
                        }
                        default:
                        {
                            throw new SupermodelException("Unrecognized pendingAction.SqlType");
                        }
                    }
                }
            });
        }
        catch (Exception)
        {
            //Rollback already happened by now
            foreach (var pendingAction in pendingActions.Where(x => x.Operation == PendingAction.OperationEnum.GenerateIdAndAdd)) pendingAction.Model!.Id = 0;
            throw;
        }
    }
    #endregion

    #region Db Initialization and Migration
    public virtual int ContextSchemaVersion => 1;
    public virtual async Task UpdateDbSchemaVersionAsync(int schemaVersion)
    {
        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var commandText = string.Format(@"UPDATE [{0}]
                                              SET 
                                                  Json = '{3}', 
                                                  BroughtFromMasterDbOnUtcTicks = {4},
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
                                              WHERE ModelTypeLogicalName = '{1}' AND ModelId = {2}",
            DataTableName, SchemaVersionModelType, 0, schemaVersion, DateTime.UtcNow.Ticks);
        await db.ExecuteAsync(commandText);
    }
    public virtual async Task<int?> GetDbSchemaVersionAsync()
    {
        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var commandText = $"SELECT * FROM [{DataTableName}] WHERE ModelTypeLogicalName = '{SchemaVersionModelType}'";
        var results = await db.QueryAsync<DataRow>(commandText);
        if (results.Count == 0) return null;
        if (results.Count > 1) return null;
        var dbSchemaVersion = int.Parse(results.Single().Json!);
        return dbSchemaVersion;
    }
    public virtual async Task<bool> InitDbAsync()
    {
        //If this is called from inside of migration, don't initialize or we get a deadlock
        var supermodelMigrationInProgressOnThisThread = (bool?)DataCallContext.LogicalGetData(SupermodelSqliteMigrationInProgressOnThisThread) ?? false;
        if (supermodelMigrationInProgressOnThisThread) return false;
            
        //check if we already initialized this run, if we did, don't bother
        if (_finishedInitializingSqlLiteContext.Contains(GetType())) return false;
        if (_startedInitializingSqlLiteContext.Contains(GetType()))
        {
            const int checkEveryMilliseconds = 250;
            var totalWait = 0;
            while (!_finishedInitializingSqlLiteContext.Contains(GetType()))
            {
                await Task.Delay(checkEveryMilliseconds);
                totalWait += checkEveryMilliseconds;
                //If this was not resolved in 12 seconds, we fail (this should be nearly instantaneous)
                if (totalWait >= 12 * 1000) throw new SupermodelException("Deadlock in InitDbAsync");
            }
            return false;
        }
        _startedInitializingSqlLiteContext.Add(GetType());
        try
        {
            var newDb = await CreateDatabaseIfNotExistsAsync();
                
            //If the database is not new, check for migrations
            if (!newDb)
            {
                var dbSchemaVersion = await GetDbSchemaVersionAsync();
                if (ContextSchemaVersion != dbSchemaVersion)
                {
                    DataCallContext.LogicalSetData(SupermodelSqliteMigrationInProgressOnThisThread, true);
                    await MigrateDbAsync(dbSchemaVersion, ContextSchemaVersion);
                    await UpdateDbSchemaVersionAsync(ContextSchemaVersion);
                    DataCallContext.LogicalSetData(SupermodelSqliteMigrationInProgressOnThisThread, false);
                }
            }
            _finishedInitializingSqlLiteContext.Add(GetType());
            return newDb;

        }
        catch (Exception)
        {
            _startedInitializingSqlLiteContext.Remove(GetType());
            throw;
        }
    }
    public virtual async Task ResetDatabaseAsync()
    {
        try
        {
            var db = new SQLiteAsyncConnection(DatabaseFilePath);

            var commandText1 = $"DROP TABLE IF EXISTS [{DataTableName}]";

            var commandText2 =
                $@"CREATE TABLE [{DataTableName}] (
                    ModelTypeLogicalName text NOT NULL,
			        ModelId integer NOT NULL,
			        Json text NOT NULL,
			        BroughtFromMasterDbOnUtcTicks integer,
			        Index0 text,
			        Index1 text,
			        Index2 text,
			        Index3 text,
			        Index4 text,
			        Index5 text,
			        Index6 text,
			        Index7 text,
			        Index8 text,
			        Index9 text,
			        Index10 integer,
			        Index11 integer,
			        Index12 integer,
			        Index13 integer,
			        Index14 integer,
			        Index15 integer,
			        Index16 integer,
			        Index17 integer,
			        Index18 integer,
			        Index19 integer,
			        Index20 real,
			        Index21 real,
			        Index22 real,
			        Index23 real,
			        Index24 real,
			        Index25 real,
			        Index26 real,
			        Index27 real,
			        Index28 real,
			        Index29 real);";

            var commandText3 = "CREATE UNIQUE INDEX UIX_Data_Data_ModelId ON Data (ModelTypeLogicalName ASC, ModelId ASC);";
            var commandText4 = "CREATE INDEX IX_Data_BroughtFromMasterDbOnUtcTicks_ModelTypeLogicalName ON Data (BroughtFromMasterDbOnUtcTicks ASC, ModelTypeLogicalName ASC);";
            var commandText5 = "CREATE INDEX IX_Data_Index0 ON Data (ModelTypeLogicalName ASC, Index0 ASC);";
            var commandText6 = "CREATE INDEX IX_Data_Index1 ON Data (ModelTypeLogicalName ASC, Index1 ASC);";
            var commandText7 = "CREATE INDEX IX_Data_Index2 ON Data (ModelTypeLogicalName ASC, Index2 ASC);";
            var commandText8 = "CREATE INDEX IX_Data_Index3 ON Data (ModelTypeLogicalName ASC, Index3 ASC);";
            var commandText9 = "CREATE INDEX IX_Data_Index4 ON Data (ModelTypeLogicalName ASC, Index4 ASC);";
            var commandText10 = "CREATE INDEX IX_Data_Index5 ON Data (ModelTypeLogicalName ASC, Index5 ASC);";
            var commandText11 = "CREATE INDEX IX_Data_Index6 ON Data (ModelTypeLogicalName ASC, Index6 ASC);";
            var commandText12 = "CREATE INDEX IX_Data_Index7 ON Data (ModelTypeLogicalName ASC, Index7 ASC);";
            var commandText13 = "CREATE INDEX IX_Data_Index8 ON Data (ModelTypeLogicalName ASC, Index8 ASC);";
            var commandText14 = "CREATE INDEX IX_Data_Index9 ON Data (ModelTypeLogicalName ASC, Index9 ASC);";
            var commandText15 = "CREATE INDEX IX_Data_Index10 ON Data (ModelTypeLogicalName ASC, Index10 ASC);";
            var commandText16 = "CREATE INDEX IX_Data_Index11 ON Data (ModelTypeLogicalName ASC, Index11 ASC);";
            var commandText17 = "CREATE INDEX IX_Data_Index12 ON Data (ModelTypeLogicalName ASC, Index12 ASC);";
            var commandText18 = "CREATE INDEX IX_Data_Index13 ON Data (ModelTypeLogicalName ASC, Index13 ASC);";
            var commandText19 = "CREATE INDEX IX_Data_Index14 ON Data (ModelTypeLogicalName ASC, Index14 ASC);";
            var commandText20 = "CREATE INDEX IX_Data_Index15 ON Data (ModelTypeLogicalName ASC, Index15 ASC);";
            var commandText21 = "CREATE INDEX IX_Data_Index16 ON Data (ModelTypeLogicalName ASC, Index16 ASC);";
            var commandText22 = "CREATE INDEX IX_Data_Index17 ON Data (ModelTypeLogicalName ASC, Index17 ASC);";
            var commandText23 = "CREATE INDEX IX_Data_Index18 ON Data (ModelTypeLogicalName ASC, Index18 ASC);";
            var commandText24 = "CREATE INDEX IX_Data_Index19 ON Data (ModelTypeLogicalName ASC, Index19 ASC);";
            var commandText25 = "CREATE INDEX IX_Data_Index20 ON Data (ModelTypeLogicalName ASC, Index20 ASC);";
            var commandText26 = "CREATE INDEX IX_Data_Index21 ON Data (ModelTypeLogicalName ASC, Index21 ASC);";
            var commandText27 = "CREATE INDEX IX_Data_Index22 ON Data (ModelTypeLogicalName ASC, Index22 ASC);";
            var commandText28 = "CREATE INDEX IX_Data_Index23 ON Data (ModelTypeLogicalName ASC, Index23 ASC);";
            var commandText29 = "CREATE INDEX IX_Data_Index24 ON Data (ModelTypeLogicalName ASC, Index24 ASC);";
            var commandText30 = "CREATE INDEX IX_Data_Index25 ON Data (ModelTypeLogicalName ASC, Index25 ASC);";
            var commandText31 = "CREATE INDEX IX_Data_Index26 ON Data (ModelTypeLogicalName ASC, Index26 ASC);";
            var commandText32 = "CREATE INDEX IX_Data_Index27 ON Data (ModelTypeLogicalName ASC, Index27 ASC);";
            var commandText33 = "CREATE INDEX IX_Data_Index28 ON Data (ModelTypeLogicalName ASC, Index28 ASC);";
            var commandText34 = "CREATE INDEX IX_Data_Index29 ON Data (ModelTypeLogicalName ASC, Index29 ASC);";

            var commandText35 =
                $@"INSERT INTO [{DataTableName}] (
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
                    '{SchemaVersionModelType}', 
                    0, 
                    '{ContextSchemaVersion}', 
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

            await db.ExecuteAsync(commandText1);
            await db.ExecuteAsync(commandText2);
            await db.ExecuteAsync(commandText3);
            await db.ExecuteAsync(commandText4);
            await db.ExecuteAsync(commandText5);
            await db.ExecuteAsync(commandText6);
            await db.ExecuteAsync(commandText7);
            await db.ExecuteAsync(commandText8);
            await db.ExecuteAsync(commandText9);
            await db.ExecuteAsync(commandText10);
            await db.ExecuteAsync(commandText11);
            await db.ExecuteAsync(commandText12);
            await db.ExecuteAsync(commandText13);
            await db.ExecuteAsync(commandText14);
            await db.ExecuteAsync(commandText15);
            await db.ExecuteAsync(commandText16);
            await db.ExecuteAsync(commandText17);
            await db.ExecuteAsync(commandText18);
            await db.ExecuteAsync(commandText19);
            await db.ExecuteAsync(commandText20);
            await db.ExecuteAsync(commandText21);
            await db.ExecuteAsync(commandText22);
            await db.ExecuteAsync(commandText23);
            await db.ExecuteAsync(commandText24);
            await db.ExecuteAsync(commandText25);
            await db.ExecuteAsync(commandText26);
            await db.ExecuteAsync(commandText27);
            await db.ExecuteAsync(commandText28);
            await db.ExecuteAsync(commandText29);
            await db.ExecuteAsync(commandText30);
            await db.ExecuteAsync(commandText31);
            await db.ExecuteAsync(commandText32);
            await db.ExecuteAsync(commandText33);
            await db.ExecuteAsync(commandText34);
            await db.ExecuteAsync(commandText35);
        }
        catch (Exception)
        {
            throw new SupermodelException("Fatal error: unable to initialize the database");
        }
    }
    public virtual async Task<bool> DataTableExistsAsync()
    {
        var db = new SQLiteAsyncConnection(DatabaseFilePath);
        var commandText = $"SELECT COUNT(*) FROM [sqlite_master] WHERE type='table' AND name='{DataTableName}';";
        var count = await db.ExecuteScalarAsync<long>(commandText);
        return count != 0;
    }
    public virtual async Task<bool> CreateDatabaseIfNotExistsAsync()
    {
        if (await DataTableExistsAsync()) return false;
        
        await ResetDatabaseAsync();
        return true;
    }
    public virtual async Task MigrateDbAsync(int? fromVersion, int toVersion)
    {
        await UnitOfWorkContext.ResetDatabaseAsync();
    }
    #endregion

    #region Properties and Contants
    public const string SupermodelSqliteMigrationInProgressOnThisThread = "SupermodelSqliteMigrationInProgressOnThisThread";
    public string SchemaVersionModelType => DataTableName + ".LocalDb.SchemaVersion";

    public virtual string DataTableName => "Data";

    //For MAUI apps return Path.Combine(FileSystem.Current.AppDataDirectory, "<filename>.db");
    //For .Net clients return Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "<filename>.db");
    public abstract string DatabaseFilePath { get; }

    // ReSharper disable InconsistentNaming
    private static readonly HashSet<Type> _finishedInitializingSqlLiteContext = new();
    private static readonly HashSet<Type> _startedInitializingSqlLiteContext = new();
    // ReSharper restore InconsistentNaming
    #endregion
}