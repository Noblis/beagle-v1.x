using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebMonk.Context;
using WebMonk.Filters.Base;
using WebMonk.Rendering.Views;
using WebMonk.Results;
using WebMonk.Session;
using WebMonk.ValueProviders;

namespace WebMonk.HttpRequestHandlers.Controllers;

public abstract class ControllerBase : IHttpRequestHandler
{
    #region IHttpRequestHandler implementation
    public abstract int Priority { get; }
    public abstract bool SaveSessionState { get; }
    
    public abstract Task<IHttpRequestHandler.HttpRequestHandlerResult> TryExecuteHttpRequestAsync(CancellationToken cancellationToken);

    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunActionsAsync(MethodInfo[] actionMethodInfos, Dictionary<string, object> routeData, CancellationToken cancellationToken)
    {
        foreach (var actionMethodInfo in actionMethodInfos)
        {
            var result = await RunActionAsync(actionMethodInfo, routeData, cancellationToken).ConfigureAwait(false);
            if (result.Success) return result;
        }
        return IHttpRequestHandler.HttpRequestHandlerResult.False;
    }
    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunAsyncActionsAsync(MethodInfo[] actionMethodInfos, Dictionary<string, object> routeData, CancellationToken cancellationToken)
    {
        foreach (var actionMethodInfo in actionMethodInfos)
        {
            var result = await RunAsyncActionAsync(actionMethodInfo, routeData, cancellationToken).ConfigureAwait(false);
            if (result.Success) return result;
        }
        return IHttpRequestHandler.HttpRequestHandlerResult.False;
    }

    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunEndPointActionsAsync(MethodInfo[] actionMethodInfos, CancellationToken cancellationToken)
    {
        foreach (var actionMethodInfo in actionMethodInfos)
        {
            var result = await RunEndpointActionAsync(actionMethodInfo, cancellationToken).ConfigureAwait(false);
            if (result.Success) return result;
        }
        return IHttpRequestHandler.HttpRequestHandlerResult.False;
    }
    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunAsyncEndPointActionsAsync(MethodInfo[] actionMethodInfos, CancellationToken cancellationToken)
    {
        foreach (var actionMethodInfo in actionMethodInfos)
        {
            var result = await RunAsyncEndPointActionAsync(actionMethodInfo, cancellationToken).ConfigureAwait(false);
            if (result.Success) return result;
        }
        return IHttpRequestHandler.HttpRequestHandlerResult.False;
    }

    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunActionAsync(MethodInfo actionMethodInfo, Dictionary<string, object> routeData, CancellationToken cancellationToken)
    {
        #region Get all the filters and set up filterContext
        var globalFilters = HttpContext.Current.WebServer.GlobalFilters.OrderBy(x => x.Priority).ToArray();
        var classFilters = GetType().GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var methodFilters = actionMethodInfo.GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var filterContext = new ActionFilterContext(this, actionMethodInfo);
        #endregion

        #region Call BeforeActionAsync on all filters
        foreach (var filter in globalFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        #region Update Route Value Provider
        var routeValueProvider = await new RouteValueProvider().InitAsync(routeData).ConfigureAwait(false);
        var valueProviders = await HttpContext.Current.ValueProviderManager.GetValueProvidersListAsync().ConfigureAwait(false);
        valueProviders.ReplaceOrInsertValueProvider(routeValueProvider, 1); //0) form-data, 1) route-data, 2) QS data
        #endregion

        #region Model Binding using value providers
        var (bindSuccessful, parameters) = await TryBindAndValidateParametersAsync(actionMethodInfo).ConfigureAwait(false);
        if (!bindSuccessful) return IHttpRequestHandler.HttpRequestHandlerResult.False;
        #endregion

        #region Execute Action Method
        var actionResult = (ActionResult)actionMethodInfo.Invoke(GetControllerInstance(), parameters);
        #endregion

        #region Call AfterActionAsync on all filters
        foreach (var filter in globalFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        return actionResult;
    }
    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunAsyncActionAsync(MethodInfo actionMethodInfo, Dictionary<string, object> routeData, CancellationToken cancellationToken)
    {
        #region Get all the filters and set up filterContext
        var globalFilters = HttpContext.Current.WebServer.GlobalFilters.OrderBy(x => x.Priority).ToArray();
        var classFilters = GetType().GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var methodFilters = actionMethodInfo.GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var filterContext = new ActionFilterContext(this, actionMethodInfo);
        #endregion

        #region Call BeforeActionAsync on all filters
        foreach (var filter in globalFilters) 
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters) 
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters) 
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        #region Update Route Value Provider
        var routeValueProvider = await new RouteValueProvider().InitAsync(routeData).ConfigureAwait(false);
        var valueProviders = await HttpContext.Current.ValueProviderManager.GetValueProvidersListAsync().ConfigureAwait(false);
        valueProviders.ReplaceOrInsertValueProvider(routeValueProvider, 1); //0) form-data, 1) route-data, 2) QS data
        #endregion

        #region Model Binding using value providers
        var (bindSuccessful, parameters) = await TryBindAndValidateParametersAsync(actionMethodInfo).ConfigureAwait(false);
        if (!bindSuccessful) return IHttpRequestHandler.HttpRequestHandlerResult.False;
        #endregion

        #region Execute Action Method
        //get the result of Task<ActionResult> after awaiting it
        var task = (Task)actionMethodInfo.Invoke(GetControllerInstance(), parameters);
        await task.ConfigureAwait(false);
        var actionResult = (ActionResult)task.GetType().GetProperty("Result")!.GetValue(task);
        #endregion
            
        #region Call AfterActionAsync on all filters
        foreach (var filter in globalFilters) 
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters) 
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters) 
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        return actionResult;
    }

    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunEndpointActionAsync(MethodInfo actionMethodInfo, CancellationToken cancellationToken)
    {
        #region Get all the filters and set up filterContext
        var globalFilters = HttpContext.Current.WebServer.GlobalFilters.OrderBy(x => x.Priority).ToArray();
        var classFilters = GetType().GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var methodFilters = actionMethodInfo.GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var filterContext = new ActionFilterContext(this, actionMethodInfo);
        #endregion

        #region Call BeforeActionAsync on all filters
        foreach (var filter in globalFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        #region Update Route Value Provider
        var routeValueProvider = new RouteValueProvider(); //in case of an EndPoint route, RouteProvider is empty
        var valueProviders = await HttpContext.Current.ValueProviderManager.GetValueProvidersListAsync().ConfigureAwait(false);
        valueProviders.ReplaceOrInsertValueProvider(routeValueProvider, 1); //0) form-data, 1) route-data, 2) QS data
        #endregion

        #region Model Binding using value providers
        var (bindSuccessful, parameters) = await TryBindAndValidateParametersAsync(actionMethodInfo).ConfigureAwait(false);
        if (!bindSuccessful) return IHttpRequestHandler.HttpRequestHandlerResult.False;
        #endregion

        #region Execute Action Method
        var actionResult = (ActionResult)actionMethodInfo.Invoke(GetControllerInstance(), parameters);
        #endregion

        #region Call AfterActionAsync on all filters
        foreach (var filter in globalFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        return actionResult;
    }
    protected virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> RunAsyncEndPointActionAsync(MethodInfo actionMethodInfo, CancellationToken cancellationToken)
    {
        #region Get all the filters and set up filterContext
        var globalFilters = HttpContext.Current.WebServer.GlobalFilters.OrderBy(x => x.Priority).ToArray();
        var classFilters = GetType().GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var methodFilters = actionMethodInfo.GetCustomAttributes().Where(x => x is IActionFilter).OrderBy(x => ((IActionFilter)x).Priority).ToArray();
        var filterContext = new ActionFilterContext(this, actionMethodInfo);
        #endregion

        #region Call BeforeActionAsync on all filters
        foreach (var filter in globalFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters)
        {
            var result = await ((IActionFilter)filter).BeforeActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        #region Update Route Value Provider
        var routeValueProvider = new RouteValueProvider();
        var valueProviders = await HttpContext.Current.ValueProviderManager.GetValueProvidersListAsync().ConfigureAwait(false);
        valueProviders.ReplaceOrInsertValueProvider(routeValueProvider, 1); //0) form-data, 1) route-data, 2) QS data
        #endregion

        #region Model Binding using value providers
        var (bindSuccessful, parameters) = await TryBindAndValidateParametersAsync(actionMethodInfo).ConfigureAwait(false);
        if (!bindSuccessful) return IHttpRequestHandler.HttpRequestHandlerResult.False;
        #endregion

        #region Execute Action Method
        //get the result of Task<ActionResult> after awaiting it
        var task = (Task)actionMethodInfo.Invoke(GetControllerInstance(), parameters);
        await task.ConfigureAwait(false);
        var actionResult = (ActionResult)task.GetType().GetProperty("Result")!.GetValue(task);
        #endregion

        #region Call AfterActionAsync on all filters
        foreach (var filter in globalFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in classFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        foreach (var filter in methodFilters)
        {
            var result = await ((IActionFilter)filter).AfterActionAsync(filterContext).ConfigureAwait(false);
            if (result.AbortProcessing) return new IHttpRequestHandler.HttpRequestHandlerResult(result.AbortFurtherRouting, result.ExecuteResultFuncAsync);
        }
        #endregion

        return actionResult;
    }

    protected virtual ControllerBase GetControllerInstance()
    {
        return (ControllerBase)Activator.CreateInstance(GetType(), null);
        //return this; //Controllers should be stateless and thread-safe
    }
    #endregion

    #region Methods
    protected RedirectResult RedirectTo(string url)
    {
        return new RedirectResult(url);
    }
    protected LocalRedirectResult RedirectToLocal(string localUrl)
    {
        return new LocalRedirectResult(localUrl);
    }
    #endregion

    #region Private Helper Methods
    protected abstract Task<(bool, object?[])> TryBindAndValidateParametersAsync(MethodInfo actionMethodInfo);
    protected virtual ImmutableList<MethodInfo> GetActionMethodsParts(Type myType)
    {
        return myType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(Render.Helper.IsActionMethod)
            .ToImmutableList();
    }
    #endregion

    #region Properties
    public TempDataDictionary TempData => HttpContext.Current.TempData;
    public SessionDictionary Session => HttpContext.Current.Session;

    protected virtual bool ValidateNullability => true;
    #endregion
}