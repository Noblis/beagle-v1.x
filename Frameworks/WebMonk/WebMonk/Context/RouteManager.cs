using System;
using System.Collections.Specialized;
using Supermodel.DataAnnotations.Exceptions;
using WebMonk.Exceptions;
using WebMonk.Extensions;
using WebMonk.ValueProviders;

namespace WebMonk.Context;

public class RouteManager : IRouteManager
{
    #region Constructors
    public RouteManager(string httpMethod, string baseUrl, string localPath, string queryString)
    {
        BaseUrl = baseUrl;
        LocalPath = localPath = localPath.ToLower().Trim();
        OverridenHttpMethod = httpMethod;
        QueryString = queryString;

        if (!localPath.StartsWith("/")) throw new ArgumentException("Path must start with /", nameof(localPath));
        if (localPath.EndsWith("/")) localPath = localPath[..^1];
            
        if (localPath.Length > 1) LocalPathParts = localPath[1..].Split('/');
        else LocalPathParts = [];
    }
    #endregion

    #region Methods
    protected virtual bool IsValidHttpMethod(string httpMethod)
    {
        httpMethod = httpMethod.ToUpper();
        if (httpMethod == "GET") return true;
        if (httpMethod == "HEAD") return true;
        if (httpMethod == "POST") return true;
        if (httpMethod == "PUT") return true;
        if (httpMethod == "DELETE") return true;
        if (httpMethod == "CONNECT") return true;
        if (httpMethod == "OPTIONS") return true;
        if (httpMethod == "TRACE") return true;
        if (httpMethod == "PATCH") return true;
        return false;
    }
    #endregion

    #region Properties
    public string BaseUrl { get; }
    public string LocalPath { get; }
    public string QueryString { get; }
    public string[] LocalPathParts { get; }
    public string LocalPathWithQueryString => LocalPath + QueryString;

    public string LocalPathWithQueryStringMinusSelectedId
    {
        get
        {
            var qsCopy = new NameValueCollection(HttpContext.Current.HttpListenerContext.Request.QueryString);
            qsCopy.Remove("selectedId");
            return LocalPath + qsCopy.ToQueryStringDictionary().ToUrlEncodedNameValuePairs();
        }
    }

    public string GetControllerFromRoute()
    {
        var controller = RouteValueProvider.GetValueOrDefault<string>("__controller__").GetCastValue<string>();
        return controller;
    }
    public string? GetAction()
    {
        var action = RouteValueProvider.GetValueOrDefault<string?>("__action__").GetCastValue<string?>();
        return action;
    }

    public RouteValueProvider RouteValueProvider 
    { 
        get
        {
            if (_routeValueProvider == null)
            {
                var valueProviders = HttpContext.Current.ValueProviderManager.GetCachedValueProvidersList();
                if (valueProviders == null) throw new SupermodelException("valueProviders == null");

                _routeValueProvider = valueProviders.GetFirstOrDefaultValueProviderOfType<RouteValueProvider>();
                if (_routeValueProvider == null) throw new SupermodelException("_routeValueProvider == null");
            }
            return _routeValueProvider;
        }
    }
    protected RouteValueProvider? _routeValueProvider;

    public string OverridenHttpMethod
    { 
        get => _overridenHttpMethod;
        set
        {
            if (!IsValidHttpMethod(value)) throw new WebMonkException($"{value} is not a valid Http Method");
            _overridenHttpMethod = value;
        }
    }
    protected string _overridenHttpMethod = "";
    #endregion
}