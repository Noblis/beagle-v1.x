using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.Exceptions;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.Multipart;
using Supermodel.Client.Backend.Utils;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.Encryptor;
using MultipartContent = Supermodel.Client.Backend.Multipart.MultipartContent;

namespace Supermodel.Client.Backend.DataContext.WebApi;

public abstract class WebApiDataContext : DataContextBase, IQueryStringProvider, IWebApiAuthorizationContext
{
    #region Contructors
    protected WebApiDataContext(AuthHeader? authHeader = null)
    {
        AuthHeader = authHeader;
    }
    #endregion

    #region IQueryStringProvider implemetation
    public string GetQueryString(object? searchBy, int? skip, int? take, string? sortBy)
    {
        var qs = new StringBuilder();
        qs.Append(searchBy.ToQueryString());
        if (sortBy != null) qs.Append("&smSortBy=" + sortBy);
        if (skip != null) qs.Append("&smSkip=" + skip);
        if (take != null) qs.Append("&smTake=" + take);
        return qs.ToString();
    }
    #endregion

    #region ValidateLogin
    public virtual async Task<LoginResult> ValidateLoginAsync<TModel>() where TModel : class, IModel
    {
        using (var httpClient = CreateHttpClient())
        {
            var url = $"{GetModelTypeLogicalName<TModel>()}/ValidateLogin";
            var dataResponse = await httpClient.GetAsync(url);
            var dataResponseContentStr = await dataResponse.Content.ReadAsStringAsync ();
            if (dataResponse.IsSuccessStatusCode)
            {
                var validateLoginResponse = JsonConvert.DeserializeObject<ValidateLoginResponse>(dataResponseContentStr);
                return new LoginResult(true, validateLoginResponse!.UserId, validateLoginResponse.UserLabel);
            }
            if (dataResponse.StatusCode == HttpStatusCode.Unauthorized) return new LoginResult(false, null, null);
            throw new SupermodelWebApiException(dataResponse.StatusCode, dataResponseContentStr);
        }
    }
    #endregion

    #region DataContext Commands
    public async Task<TOutput?> ExecutePostAsync<TInput, TOutput>(string url, TInput input)
        where TInput : class, new()
        where TOutput : class, new()
    {
        using (var httpClient = CreateHttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}{url}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);
            var responseContentStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) ThrowSupermodelWebApiException(response.StatusCode, responseContentStr);
            var output = JsonConvert.DeserializeObject<TOutput>(responseContentStr);
            return output;
        }
    }
    #endregion

    #region DataContext Reads
    public override async Task<TModel?> GetByIdOrDefaultAsync<TModel>(long id) where TModel : class
    {
        var url = $"{GetModelTypeLogicalName<TModel>()}/{id}";
        return await GetJsonObjectAsync<TModel>(url);
    }
    public override async Task<List<TModel>> GetAllAsync<TModel>(int? skip = null, int? take = null)
    {
        var url = $"{GetModelTypeLogicalName<TModel>()}/All?";
        if (skip != null) url += "&smSkip=" + skip;
        if (take != null) url += "&smTake=" + take;
        return await GetJsonObjectsAsync<TModel>(url);
    }
    public override async Task<long> GetCountAllAsync<TModel>(int? skip = null, int? take = null)
    {
        var url = $"{GetModelTypeLogicalName<TModel>()}/CountAll?";
        if (skip != null) url += "&smSkip=" + skip;
        if (take != null) url += "&smTake=" + take;
        return await GetCountAsync(url);
    }
    #endregion

    #region DataContext Queries
    public override async Task<List<TModel>> GetWhereAsync<TModel>(object? searchBy, string? sortBy = null, int? skip = null, int? take = null)
    {
        var url = $"{GetModelTypeLogicalName<TModel>()}/Where?{searchBy.ToQueryString()}";
        if (sortBy != null) url += "&smSortBy=" + sortBy;
        if (skip != null) url += "&smSkip=" + skip;
        if (take != null) url += "&smTake=" + take;
        return await GetJsonObjectsAsync<TModel>(url);
    }
    public override async Task<long> GetCountWhereAsync<TModel>(object? searchBy, int? skip = null, int? take = null)
    {
        var url = $"{GetModelTypeLogicalName<TModel>()}/CountWhere?{searchBy.ToQueryString()}";
        if (skip != null) url += "&smSkip=" + skip;
        if (take != null) url += "&smTake=" + take;
        return await GetCountAsync(url);
    }
    #endregion

    #region DataContext Save Changes
    public override async Task SaveChangesInternalAsync(List<PendingAction> pendingActions)
    {
        using (var httpClient = CreateHttpClient())
        {
            HttpRequestMessage request;
            if (pendingActions.Count == 1)
            {
                request = pendingActions.Single().GenerateHttpRequest(BaseUrl, this, false);
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "$batch") { Content = new MultipartContent("mixed") };
                foreach (var pendingAction in pendingActions) ((MultipartContent)request.Content).Add(new HttpMessageContent(pendingAction.GenerateHttpRequest(BaseUrl, this, true)));
            }

            var response = await httpClient.SendAsync(request);
            var responseContentStr = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                //this is a special case for when an object does not exist and it's ok
                if (response.StatusCode == HttpStatusCode.NotFound && pendingActions.Count == 1 && pendingActions.Single().Operation == PendingAction.OperationEnum.DelayedGetByIdOrDefault)
                {
                    //by this we indicate that this is not a transport error - it's an indication that object with id does not exist
                    responseContentStr = null;
                }
                else if (response.StatusCode == HttpStatusCode.ExpectationFailed) //If validation error(s)
                {
                    var validationError = JsonConvert.DeserializeObject<SupermodelDataContextValidationException.ValidationError>(responseContentStr);
                    validationError!.FailedAction = pendingActions.Single();
                    ThrowSupermodelValidationException(validationError);
                }
                else
                {
                    ThrowSupermodelWebApiException(response.StatusCode, responseContentStr);
                }
            }

            if (pendingActions.Count == 1)
            {
                pendingActions.Single().ProcessHttpResponse(responseContentStr);
            }
            else
            {
                var streamProvider = await response.Content.ReadAsMultipartAsync();
                if (pendingActions.Count != streamProvider.Contents.Count) throw new SupermodelException("Response does not match the request");
                var validationErrors = new List<SupermodelDataContextValidationException.ValidationError>();
                for (var i = 0; i < pendingActions.Count; i++)
                {
                    var dataResponse = await streamProvider.Contents[i].ReadAsHttpResponseMessageAsync();
                    var dataResponseContentStr = await dataResponse.Content.ReadAsStringAsync();
                    if (!dataResponse.IsSuccessStatusCode)
                    {
                        //this is a special case for when an object does not exists and it's ok
                        if (dataResponse.StatusCode == HttpStatusCode.NotFound && pendingActions[i].Operation == PendingAction.OperationEnum.DelayedGetByIdOrDefault)
                        {
                            //by this we indicate that this is not an transport error - it's an indication that object with id does not exists
                            pendingActions[i].ProcessHttpResponse(null);
                        }
                        else if (dataResponse.StatusCode == HttpStatusCode.ExpectationFailed) //If validation error(s)
                        {
                            var validationError = JsonConvert.DeserializeObject<SupermodelDataContextValidationException.ValidationError>(dataResponseContentStr)!;
                            validationErrors.Add(validationError);
                        }
                        else
                        {
                            ThrowSupermodelWebApiException(dataResponse.StatusCode, dataResponseContentStr);
                        }
                    }
                    else
                    {
                        pendingActions[i].ProcessHttpResponse(dataResponseContentStr);
                    }
                }
                if (validationErrors.Any()) ThrowSupermodelValidationException(validationErrors);
            }
        }
    }
    #endregion

    #region Helpers
    protected virtual HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(AcceptHeader));
        if (AuthHeader != null) httpClient.DefaultRequestHeaders.Add(AuthHeader.HeaderName, AuthHeader.AuthToken);
        return httpClient;
    }
    protected void ThrowSupermodelWebApiException(HttpStatusCode statusCode, string content)
    {
        PrepareForThrowingException();
        throw new SupermodelWebApiException(statusCode, content);
    }
    protected async Task<List<TModel>> GetJsonObjectsAsync<TModel>(string url) where TModel : class, IModel, new()
    {
        var models = await GetJsonAnyObjectsAsync<TModel>(url);
        foreach (var model in models)
        {
            model.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
            model.AfterLoad();
            ManagedModels.Add(new ManagedModel(model));
        }
        return models;
    }
    protected async Task<TModel?> GetJsonObjectAsync<TModel>(string url) where TModel : class, IModel, new()
    {
        var model = await GetJsonAnyObjectAsync<TModel>(url);
        if (model != null)
        {
            model.BroughtFromMasterDbOnUtc = DateTime.UtcNow;
            model.AfterLoad();
            ManagedModels.Add(new ManagedModel(model));
        }
        return model;
    }
    protected async Task<long> GetCountAsync(string url)
    {
        using (var httpClient = CreateHttpClient())
        {
            var dataResponse = await httpClient.GetAsync(url);
            var dataResponseContentStr = await dataResponse.Content.ReadAsStringAsync ();
            if (!dataResponse.IsSuccessStatusCode) throw new SupermodelWebApiException(dataResponse.StatusCode, dataResponseContentStr);
            var count = JsonConvert.DeserializeObject<long>(dataResponseContentStr);
            return count;
        }
    }
    protected async Task<List<T>> GetJsonAnyObjectsAsync<T>(string url) where T: class
    {
        using (var httpClient = CreateHttpClient())
        {
            var dataResponse = await httpClient.GetAsync(url);
            var responseContentStr = await dataResponse.Content.ReadAsStringAsync ();
            if (!dataResponse.IsSuccessStatusCode) throw new SupermodelWebApiException(dataResponse.StatusCode, responseContentStr);
            var models = JsonConvert.DeserializeObject<List<T>>(responseContentStr)!;
            return models;
        }
    }
    protected async Task<T?> GetJsonAnyObjectAsync<T>(string url)  where T: class
    {
        using (var httpClient = CreateHttpClient())
        {
            var dataResponse = await httpClient.GetAsync(url);
            if (dataResponse.StatusCode == HttpStatusCode.NotFound) return null;
            var dataResponseContentStr = await dataResponse.Content.ReadAsStringAsync ();
            if (!dataResponse.IsSuccessStatusCode) throw new SupermodelWebApiException (dataResponse.StatusCode, dataResponseContentStr);
            var model = JsonConvert.DeserializeObject<T>(dataResponseContentStr);
            return model;
        }
    }
    protected async Task<TScalarValue?> GetScalarValueAsync<TScalarValue>(string url)
    {
        using (var httpClient = CreateHttpClient())
        {
            var dataResponse = await httpClient.GetAsync(url);
            var dataResponseContentStr = await dataResponse.Content.ReadAsStringAsync ();
            if (!dataResponse.IsSuccessStatusCode) throw new SupermodelWebApiException(dataResponse.StatusCode, dataResponseContentStr);
            var count = JsonConvert.DeserializeObject<TScalarValue>(dataResponseContentStr);
            return count;
        }
    }
    #endregion

    #region Properties & Constants
    public const string AcceptHeader = "application/json";
    public AuthHeader? AuthHeader { get; set; }
    public abstract string BaseUrl { get; }
    #endregion
}