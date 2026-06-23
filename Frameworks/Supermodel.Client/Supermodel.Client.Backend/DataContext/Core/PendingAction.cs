using System.Text;
using Newtonsoft.Json;
using Supermodel.Client.Backend.DataContext.Sqlite;
using Supermodel.Client.Backend.DataContext.WebApi;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.UnitOfWork;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.ReflectionMapper;

namespace Supermodel.Client.Backend.DataContext.Core;

public class PendingAction 
{
    #region Embedded Enums
    public enum OperationEnum { AddWithExistingId, GenerateIdAndAdd, Update, Delete, DelayedGetById, DelayedGetByIdOrDefault, DelayedGetAll, DelayedGetCountAll, DelayedGetWhere, DelayedGetCountWhere, AddOrUpdate }
    public enum OperationSqlType { SingleResultQuery, ListResultQuery, ScalarResultQuery, NoQueryResult }
    #endregion

    #region Constructors
    public PendingAction()
    {
        Disabled = false;
    }
    public PendingAction(PendingAction other) //copy constructor
    {
        Disabled = other.Disabled;
        Operation = other.Operation;
        ModelType = other.ModelType;
        ModelId = other.ModelId;
        OriginalModelId = other.OriginalModelId;
        Model = other.Model;
        DelayedValue = other.DelayedValue;
        SearchBy = other.SearchBy;
        Skip = other.Skip;
        Take = other.Take;
        SortBy = other.SortBy;
    }
    #endregion

    #region Methods
    public PendingAction Validate()
    {
        if (!IsValid()) throw new SupermodelException("Invalid PendingAction");
        return this;
    }
    public bool IsValid()
    {
        if (ModelType == null) return false;

        switch (Operation)
        {
            case OperationEnum.GenerateIdAndAdd:
            {
                if (ModelId != 0) return false;
                if (Model == null) return false;
                break;
            }
            case OperationEnum.AddWithExistingId:
            case OperationEnum.AddOrUpdate:
            {
                if (ModelId == 0) return false;
                if (Model == null) return false;
                break;
            }
            case OperationEnum.Delete:
            {
                if (ModelId == 0) return false;
                if (OriginalModelId != ModelId) return false;
                if (Model == null) return false;
                break;
            }
            case OperationEnum.Update:
            {
                if (ModelId == 0) return false;
                if (OriginalModelId == 0) return false;
                if (Model == null) return false;
                break;
            }
            case OperationEnum.DelayedGetById:
            case OperationEnum.DelayedGetByIdOrDefault:
            {
                if (ModelId == 0) return false;
                if (DelayedValue == null) return false;
                break;
            }
            case OperationEnum.DelayedGetAll:
            case OperationEnum.DelayedGetWhere:
            case OperationEnum.DelayedGetCountAll:
            case OperationEnum.DelayedGetCountWhere:
            {
                if (DelayedValue == null) return false;
                break;
            }
            default:
            {
                throw new SupermodelException("Unsupported Operation");
            }
        }
        return true;
    }
    public bool IsReadOnlyAction 
    {
        get
        {
            switch (Operation)
            {
                case OperationEnum.AddWithExistingId:
                case OperationEnum.GenerateIdAndAdd:
                case OperationEnum.Update:
                case OperationEnum.Delete:
                case OperationEnum.AddOrUpdate:
                    return false;

                case OperationEnum.DelayedGetById:
                case OperationEnum.DelayedGetByIdOrDefault:
                case OperationEnum.DelayedGetAll:
                case OperationEnum.DelayedGetWhere:
                case OperationEnum.DelayedGetCountAll:
                case OperationEnum.DelayedGetCountWhere:
                    return true;

                default:
                    throw new SupermodelException("Unsupported Operation");
            }
        }
    }
    public OperationSqlType SqlType
    {
        get
        {
            switch (Operation)
            {
                case OperationEnum.AddWithExistingId:
                case OperationEnum.GenerateIdAndAdd:
                case OperationEnum.Update:
                case OperationEnum.Delete:
                case OperationEnum.AddOrUpdate:
                    return OperationSqlType.NoQueryResult;

                case OperationEnum.DelayedGetById:
                case OperationEnum.DelayedGetByIdOrDefault:
                    return OperationSqlType.SingleResultQuery;

                case OperationEnum.DelayedGetAll:
                case OperationEnum.DelayedGetWhere:
                    return OperationSqlType.ListResultQuery;

                case OperationEnum.DelayedGetCountAll:
                case OperationEnum.DelayedGetCountWhere:
                    return OperationSqlType.ScalarResultQuery;

                default:
                    throw new SupermodelException("Unsupported Operation");
            }
        }
    }
    #endregion

    #region WebApi Methods
    public HttpRequestMessage GenerateHttpRequest(string baseUrl, IQueryStringProvider queryStringProvider, bool includeAuthHeader)
    {
        HttpRequestMessage message; 
        switch (Operation)
        {
            case OperationEnum.AddWithExistingId:
            {
                throw new SupermodelException("AddWithExistingId is not supported by WebApiContext -- only GenerateIdAndAdd");
            }
            case OperationEnum.GenerateIdAndAdd:
            {
                if (ModelId != 0 || Model!.Id != 0) throw new SupermodelException("When adding a new Model Id must be equal 0.");
                message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}")
                {
                    //Content = new StringContent(JsonConvert.SerializeObject(Model.PerpareForSerializingForMasterDb()), Encoding.UTF8, ContentType)
                    Content = new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, ContentType),
                };
                break;
            }
            case OperationEnum.Update:
            {
                if (ModelId == 0 || Model!.Id == 0 || ModelId != Model.Id) throw new SupermodelException("When updating a ModelIds must be not equal 0 and equal to each other");
                message = new HttpRequestMessage(HttpMethod.Put, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/{ModelId}")
                {
                    //Content = new StringContent(JsonConvert.SerializeObject(Model.PerpareForSerializingForMasterDb()), Encoding.UTF8, ContentType)
                    Content = new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, ContentType)
                };
                break;
            }
            case OperationEnum.Delete:
            {
                message = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/{ModelId}");
                break;
            }
            case OperationEnum.DelayedGetById:
            case OperationEnum.DelayedGetByIdOrDefault:
            {
                message = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/{ModelId}");
                break;
            }
            case OperationEnum.DelayedGetAll:
            {
                message = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/All");
                break;
            }
            case OperationEnum.DelayedGetCountAll:
            {
                message = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/CountAll");
                break;
            }
            case OperationEnum.DelayedGetWhere:
            {
                message = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/Where?{queryStringProvider.GetQueryString(SearchBy, Skip, Take, SortBy)}");
                break;
            }
            case OperationEnum.DelayedGetCountWhere:
            {
                message = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}{DataContextBase.GetModelTypeLogicalName(ModelType)}/CountWhere?{queryStringProvider.GetQueryString(SearchBy, Skip, Take, SortBy)}");
                break;
            }

            // ReSharper disable once RedundantCaseLabel
            case OperationEnum.AddOrUpdate:
            default:
                throw new SupermodelException("Unsupported Operation");
        }

        if (includeAuthHeader && UnitOfWorkContextCore.CurrentDataContext is IWebApiAuthorizationContext) 
        {
            var authHeader = UnitOfWorkContext.AuthHeader;
            if (authHeader != null) message.Headers.Add(authHeader.HeaderName, authHeader.AuthToken);
        }
            
        return message;
    }
    public void ProcessHttpResponse(string? responseContentStr)
    {
        switch (Operation)
        {
            case OperationEnum.AddWithExistingId:
            {
                throw new SupermodelException("AddWithExistingId is not supported by WebApiContext -- only GenerateIdAndAdd");
            }
            case OperationEnum.GenerateIdAndAdd:
            {
                Model!.Id = long.Parse(responseContentStr!);
                Model.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
                break;
            }
            case OperationEnum.Update:
            {
                Model!.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
                break;
            }
            case OperationEnum.Delete:
            {
                //Do nothing
                break;
            }
            case OperationEnum.DelayedGetById:
            {
                if (string.IsNullOrEmpty(responseContentStr))
                {
                    throw new SupermodelException("DelayedGetById: no object exists with id = " + ModelId);
                }
                else
                {
                    var model = (IModel)JsonConvert.DeserializeObject(responseContentStr, ModelType!)!;
                    model.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
                    model.AfterLoad();
                    DelayedValue!.SetValue(model);
                }
                break;
            }
            case OperationEnum.DelayedGetByIdOrDefault:
            {
                if (string.IsNullOrEmpty(responseContentStr))
                {
                    DelayedValue!.SetValue(null);
                }
                else
                {
                    var model = (IModel)JsonConvert.DeserializeObject(responseContentStr, ModelType!)!;
                    model.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
                    model.AfterLoad();
                    DelayedValue!.SetValue(model);
                }
                break;
            }
            case OperationEnum.DelayedGetAll:
            case OperationEnum.DelayedGetWhere:
            {
                var models = (IEnumerable<IModel>)JsonConvert.DeserializeObject(responseContentStr!, typeof(List<>).MakeGenericType(ModelType!))!;
                foreach (var model in models)
                {
                    model.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
                    model.AfterLoad();
                }
                DelayedValue!.SetValue(models);
                break;
            }
            case OperationEnum.DelayedGetCountAll:
            case OperationEnum.DelayedGetCountWhere:
            {
                var count = JsonConvert.DeserializeObject<long>(responseContentStr!);
                DelayedValue!.SetValue(count);
                break;
            }
            // ReSharper disable once RedundantCaseLabel
            case OperationEnum.AddOrUpdate:
            default:
            {
                throw new SupermodelException("Unsupported Operation");
            }
        }
    }
    #endregion

    #region Sqlite Methods
    public string GenerateSql(string dataTableName, ISqlQueryProvider sqlQueryProvider)
    {
        switch (Operation)
        {
            case OperationEnum.AddWithExistingId:
            case OperationEnum.GenerateIdAndAdd:
            {
                return new DataRow(Model!, OriginalModelId, sqlQueryProvider).GenerateSqlInsert(dataTableName);
            }
            case OperationEnum.AddOrUpdate:
            {
                return new DataRow(Model!, OriginalModelId, sqlQueryProvider).GenerateSqlInsertOrReplace(dataTableName);
            }
            case OperationEnum.Update:
            {
                return new DataRow(Model!, OriginalModelId, sqlQueryProvider).GenerateSqlUpdate(dataTableName);
            }
            case OperationEnum.Delete:
            {
                return $@"DELETE FROM [{dataTableName}] WHERE ModelTypeLogicalName = '{DataContextBase.GetModelTypeLogicalName(ModelType)}' AND ModelId = {ModelId}";
            }
            case OperationEnum.DelayedGetById:
            case OperationEnum.DelayedGetByIdOrDefault:
            {
                return $"SELECT * FROM [{dataTableName}] WHERE ModelTypeLogicalName = '{DataContextBase.GetModelTypeLogicalName(ModelType)}' AND ModelId = {ModelId}";
            }
            case OperationEnum.DelayedGetAll:
            {
                return $"SELECT * FROM [{dataTableName}] WHERE ModelTypeLogicalName = '{DataContextBase.GetModelTypeLogicalName(ModelType)}'";
            }
            case OperationEnum.DelayedGetWhere:
            {
                //var whereClause = sqlQueryProvider.GetWhereClause(SearchBy, SortBy);
                var whereClause = (string?)sqlQueryProvider.ExecuteGenericMethod("GetWhereClause", [ModelType!], SearchBy, SortBy);

                if (whereClause == null) throw new SupermodelException("Must override GetWhereClause before running queries on localDb");

                //var fullWhereClause = whereClause + sqlQueryProvider.GetSkipAndTakeForWhereClause(Skip, Take);
                var fullWhereClause = whereClause + sqlQueryProvider.ExecuteGenericMethod("GetSkipAndTakeForWhereClause", [ModelType!], Skip, Take);

                return $"SELECT * FROM [{dataTableName}] WHERE ModelTypeLogicalName = '{DataContextBase.GetModelTypeLogicalName(ModelType)}' {fullWhereClause}";
            }
            case OperationEnum.DelayedGetCountAll:
            {
                return $"SELECT COUNT(*) FROM [{dataTableName}] WHERE ModelTypeLogicalName = '{DataContextBase.GetModelTypeLogicalName(ModelType)}'";
            }
            case OperationEnum.DelayedGetCountWhere:
            {
                //var whereClause = sqlQueryProvider.GetWhereClause(SearchBy, SortBy);
                var whereClause = (string?)sqlQueryProvider.ExecuteGenericMethod("GetWhereClause", [ModelType!], SearchBy, SortBy);

                if (whereClause == null) throw new SupermodelException("Must override GetWhereClause before running queries on localDb");

                //var fullWhereClause = whereClause + sqlQueryProvider.GetSkipAndTakeForWhereClause(Skip, Take);
                var fullWhereClause = whereClause + sqlQueryProvider.ExecuteGenericMethod("GetSkipAndTakeForWhereClause", [ModelType!], Skip, Take);

                return $"SELECT COUNT(*) FROM [{dataTableName}] WHERE ModelTypeLogicalName = '{DataContextBase.GetModelTypeLogicalName(ModelType)}' AND {fullWhereClause}";
            }
            default:
            {
                throw new SupermodelException("Unsupported Operation");
            }
        }
    }
    // ReSharper disable UnusedParameter.Global
    public void ProcessDatabaseResponseAsync(string dataTableName, object? response, ISqlQueryProvider sqlQueryProvider) 
    // ReSharper restore UnusedParameter.Global
    {
        switch (Operation)
        {
            case OperationEnum.AddWithExistingId:
            case OperationEnum.GenerateIdAndAdd:
            case OperationEnum.AddOrUpdate:
            case OperationEnum.Update:
            case OperationEnum.Delete:
            {
                //we have nothing to process
                return;
            }
            case OperationEnum.DelayedGetById:
            case OperationEnum.DelayedGetByIdOrDefault:
            {
                var results = (List<DataRow>)response!;
                if (results.Count == 0)
                {
                    if (Operation == OperationEnum.DelayedGetById) throw new SupermodelException("DelayedGetById: no object exists with id = " + ModelId);
                    DelayedValue!.SetValue(null);
                    return;
                }
                if (results.Count > 1) throw new Exception("GetByIdOrAsync or GetByIdOrDefaultAsync brought back more than one record");
                var model = results.Single().GetModel(ModelType!);
                DelayedValue!.SetValue(model);
                return;
            }
            case OperationEnum.DelayedGetAll:
            case OperationEnum.DelayedGetWhere:
            {
                var results = (List<DataRow>)response!;
                var models = ReflectionHelper.CreateGenericType(typeof (List<>), ModelType!);
                foreach (var result in results)
                {
                    var model = result.GetModel(ModelType!);
                    models.ExecuteMethod("Add", model);
                }
                DelayedValue!.SetValue(models);
                return;
            }
            case OperationEnum.DelayedGetCountAll:
            case OperationEnum.DelayedGetCountWhere:
            {
                var responseLong = (long)response!;
                DelayedValue!.SetValue(responseLong);
                return;
            }
            default:
            {
                throw new SupermodelException("Unrecognized pendingAction.SqlType");
            }
        }
    }
    #endregion

    #region Properties and Constants
    public bool Disabled { get; set; }
    public OperationEnum Operation { get; set; }
    public Type? ModelType { get; set; }
    public long ModelId { get; set; }
    public long OriginalModelId { get; set; }
    public IModel? Model { get; set; }
    public DelayedValue? DelayedValue { get; set; }
    public object? SearchBy { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public string? SortBy { get; set; }

    public const string ContentType = "application/json";
    #endregion
}