using Namotion.Reflection;
using Newtonsoft.Json;
using Supermodel.DataAnnotations.Validations;
using Supermodel.ReflectionMapper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebMonk.Context;
using WebMonk.Exceptions;
using WebMonk.Extensions;
using WebMonk.HttpRequestHandlers.Controllers.Modules;
using WebMonk.Misc;
using WebMonk.ModeBinding;
using WebMonk.Rendering.Views;
using WebMonk.Results;
using WebMonk.ValueProviders;

namespace WebMonk.HttpRequestHandlers.Controllers;

public abstract class ApiController : ControllerBase
{
    #region IHttpRequestHandler implementation
    public override int Priority => 400;
    public override bool SaveSessionState => false;

    public override async Task<IHttpRequestHandler.HttpRequestHandlerResult> TryExecuteHttpRequestAsync(CancellationToken cancellationToken)
    {
        var myType = GetType();
        var actionMethodsParts = GetActionMethodsParts(myType);

        var overridenHttpMethod = HttpContext.Current.RouteManager.OverridenHttpMethod;

        if (this is ApiModule)
        {
            //Handle EndPoint attribute routing
            var localPath = HttpContext.Current.RouteManager.LocalPath;

            var endPointMethodInfos = actionMethodsParts.Where(x => x.Name.StartsWith(overridenHttpMethod, StringComparison.InvariantCultureIgnoreCase) && !x.Name.EndsWith("Async") && x.GetAttribute<EndPointAttribute>()?.Url.Equals(localPath, StringComparison.InvariantCultureIgnoreCase) == true).ToArray();
            if (endPointMethodInfos.Length > 0) return await RunEndPointActionsAsync(endPointMethodInfos, cancellationToken).ConfigureAwait(false);

            var asyncEndPointMethodInfos = actionMethodsParts.Where(x => x.Name.StartsWith(overridenHttpMethod, StringComparison.InvariantCultureIgnoreCase) && x.Name.EndsWith("Async") && x.GetAttribute<EndPointAttribute>()?.Url.Equals(localPath, StringComparison.InvariantCultureIgnoreCase) == true).ToArray();
            if (asyncEndPointMethodInfos.Length > 0) return await RunAsyncEndPointActionsAsync(endPointMethodInfos, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            //Handle standard controller/action routing
            var controllerPart = myType.GetApiControllerName();
            var localParts = HttpContext.Current.RouteManager.LocalPathParts;
            if (localParts.Length < 2) return IHttpRequestHandler.HttpRequestHandlerResult.False;
            if (!localParts[0].Equals("api", StringComparison.InvariantCultureIgnoreCase)) return IHttpRequestHandler.HttpRequestHandlerResult.False;
            if (!controllerPart.Equals(localParts[1], StringComparison.InvariantCultureIgnoreCase)) return IHttpRequestHandler.HttpRequestHandlerResult.False;

            string? action;
            Dictionary<string, object> routeData;
            var controller = localParts[1];
            if (localParts.Length >= 3)
            {
                if (long.TryParse(localParts[2], out _))
                {
                    // /api/student/1
                    action = null;
                    routeData = new Dictionary<string, object> { { "__controller__", controller }, { "id", localParts[2] } };
                }
                else
                {
                    // /api/student/all or /api/student/detail/1
                    action = localParts[2];
                    if (localParts.Length >= 4) routeData = new Dictionary<string, object> { { "__controller__", controller }, { "__action__", action }, { "id", localParts[3] } };
                    else routeData = new Dictionary<string, object> { { "__controller__", controller }, { "__action__", action } };
                }
            }
            else //localParts.Length cannot be less than 2, we checked for that earlier
            {
                action = null;
                routeData = new Dictionary<string, object> { { "__controller__", controller } };
            }

            var actionMethodInfos = actionMethodsParts.Where(x => $"{overridenHttpMethod}{action}".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase) && !x.HasAttribute<EndPointAttribute>()).ToArray();
            if (actionMethodInfos.Length > 0) return await RunActionsAsync(actionMethodInfos, routeData, cancellationToken).ConfigureAwait(false);

            var asyncActionMethodInfos = actionMethodsParts.Where(x => $"{overridenHttpMethod}{action}Async".Equals(x.Name, StringComparison.InvariantCultureIgnoreCase) && !x.HasAttribute<EndPointAttribute>()).ToArray();
            if (asyncActionMethodInfos.Length > 0) return await RunAsyncActionsAsync(asyncActionMethodInfos, routeData, cancellationToken).ConfigureAwait(false);
        }

        return IHttpRequestHandler.HttpRequestHandlerResult.False;
    }
    #endregion

    #region Overrides
    protected override async Task<(bool, object?[])> TryBindAndValidateParametersAsync(MethodInfo actionMethodInfo)
    {
        var valueProviders = await HttpContext.Current.ValueProviderManager.GetValueProvidersListAsync().ConfigureAwait(false);
            
        var parametersList = new List<object?>();
            
        var parametersInfos = actionMethodInfo.GetParameters();
        foreach (var parameterInfo in parametersInfos)
        {
            var parametersType = parameterInfo.ParameterType;
            var modelBinder = HttpContext.Current.StaticModelBinderManager.GetStaticModelBinder();
                
            var parameterValue = Type.Missing;
                
            //if class or a struct, we don't need an extra prefix 
            if (InternalTypeExt.IsComplexType(parameterInfo.ParameterType))
            {
                //first try to bind to all value providers (query string)
                using(HttpContext.Current.PrefixManager.NewPrefix(parameterInfo.Name, null))
                {
                    parameterValue = await modelBinder.BindExistingModelAsync(parametersType, parametersType, parameterValue, valueProviders).ConfigureAwait(false);
                }

                //Then we bind to body as json
                var messageBodyValueProvider = valueProviders.GetFirstOrDefaultValueProviderOfType<MessageBodyValueProvider>() ?? throw new WebMonkException("Unable to find MessageBodyValueProvider");
                var bodyResult = messageBodyValueProvider.GetValueOrDefault(""); //get the entire body
                if (!bodyResult.ValueMissing && bodyResult.Value != null)
                {
                    var body = bodyResult.GetCastValue<string>();
                    if (!string.IsNullOrEmpty(body)) parameterValue = JsonConvert.DeserializeObject(body, parameterInfo.ParameterType);
                }
            }
            else
            {
                using(HttpContext.Current.PrefixManager.NewPrefix(parameterInfo.Name, null))
                {
                    try
                    {
                        parameterValue = await modelBinder.BindNewModelAsync(parametersType, parametersType, valueProviders).ConfigureAwait(false);
                    }
                    catch (WebMonkInvalidFormatException)
                    {
                        return (false, []);
                    }
                }
            }
                
            if (parameterValue == null)
            { 
                if (parameterInfo.ToContextualParameter().Nullability == Nullability.NotNullable) return (false, parametersList.ToArray());
            }
            else if (parameterValue == Type.Missing)
            {
                if (!parameterInfo.IsOptional) return(false, []);
            }
            else
            {
                var nVrl = new ValidationResultList(); //nullability validation result list
                if (ValidateNullability && !NullabilityHelper.TryValidateObjectNullability(parameterValue, nVrl))
                {
                    HttpContext.Current.ValidationResultList.AddValidationResultList(nVrl);
                }
                    
                var vrl = new ValidationResultList();
                if (!await AsyncValidator.TryValidateObjectAsync(parameterValue, new ValidationContext(parameterValue), vrl).ConfigureAwait(false))
                {
                    HttpContext.Current.ValidationResultList.AddValidationResultList(vrl);
                }
            }

            parametersList.Add(parameterValue);
        }
        return (true, parametersList.ToArray());
    }        
    #endregion

    #region Methods
    public virtual Task<bool> TryUpdateModelAsync(object model, List<IValueProvider>? valueProviders = null)
    {
        return HttpContext.Current.StaticModelBinderManager.TryUpdateApiModelAsync(model, valueProviders);
    }

    protected virtual LocalRedirectResult RedirectToAction<T>(Expression<Action<T>> action, NameValueCollection? queryString)  where T : ApiController
    {
        return RedirectToAction(action, queryString?.ToQueryStringDictionary());
    }
    protected virtual LocalRedirectResult RedirectToAction<T>(Expression<Action<T>> action, QueryStringDict? queryStringDict = null) where T : ApiController
    {
        return new LocalRedirectResult(Render.Helper.UrlForApiAction(action, queryStringDict));
    }        

    protected virtual LocalRedirectResult RedirectToAction(string action, NameValueCollection? queryString)
    {
        return RedirectToAction(action, queryString?.ToQueryStringDictionary());
    }
    protected virtual LocalRedirectResult RedirectToAction(string action, QueryStringDict? queryStringDict = null)
    {
        return RedirectToAction(action, "", queryStringDict);
    }
        
    protected virtual LocalRedirectResult RedirectToAction(string action, string id, NameValueCollection? queryString)
    {
        return RedirectToAction(action, id, queryString?.ToQueryStringDictionary());
    }
    protected virtual LocalRedirectResult RedirectToAction(string action, string id, QueryStringDict? queryStringDict = null)
    {
        var controller = GetType().GetApiControllerName();
        return RedirectToAction(controller, action, id, queryStringDict);
    }
        
    protected virtual LocalRedirectResult RedirectToAction(string controller, string action, string id, NameValueCollection? queryString)
    {
        return RedirectToAction(controller, action, id, queryString?.ToQueryStringDictionary());
    }
    protected virtual LocalRedirectResult RedirectToAction(string controller, string action, string id, QueryStringDict? queryStringDict = null)
    {
        return new LocalRedirectResult(Render.Helper.UrlForApiAction(controller, action, id, queryStringDict));
    }
    #endregion
}