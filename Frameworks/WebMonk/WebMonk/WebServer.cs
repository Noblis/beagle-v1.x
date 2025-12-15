using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.ReflectionMapper;
using WebMonk.Context;
using WebMonk.Context.WMHttpListenerObjects;
using WebMonk.Exceptions;
using WebMonk.Filters.Base;
using WebMonk.HttpRequestHandlers;
using WebMonk.HttpRequestHandlers.Controllers;
using WebMonk.Rendering.Views;
using WebMonk.Results;
using WebMonk.Session;
using WebMonk.Startup;
using WebMonk.ValueProviders;

namespace WebMonk;

public class WebServer
{
    #region Constructors
    public WebServer(int httpPort, string navigationBaseUrl, Assembly[]? appAssemblies = null)
    {
        ListeningBaseUrl =$"http://+:{httpPort}/";
        Prompt = $"Supermodel WebMonk Server is Listening on {ListeningBaseUrl} ...";
            
        if (!navigationBaseUrl.EndsWith("/")) navigationBaseUrl += "/";
        NavigatingBaseUrl = navigationBaseUrl;

        HttpListener = new HttpListener 
        { 
            AuthenticationSchemes = AuthenticationSchemes.Anonymous,
            Prefixes = { ListeningBaseUrl }
        };
        AppAssemblies = appAssemblies ?? AppDomain.CurrentDomain.GetAllAssemblies();
            
        //It is ok because this is the last method in the constructor
        // ReSharper disable once VirtualMemberCallInConstructor
        SortedHttpRequestHandlers = GetAndSortHttpRequestHandlers(AppAssemblies);

        //Check for warnings
        var mvcControllers = SortedHttpRequestHandlers.Where(x => x is MvcController);
        // ReSharper disable once PossibleMultipleEnumeration
        var duplicateMvcControllers = mvcControllers.GroupBy(x => x.GetType().Name).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
        foreach (var duplicateMvcController in duplicateMvcControllers)
        {
            Console.WriteLine($"WARNING! Found multiple mvc controllers named {duplicateMvcController}:");
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var duplicate in mvcControllers.Where(x => x.GetType().Name == duplicateMvcController))
            {
                Console.WriteLine($" - {duplicate.GetType().FullName}");
            }
        }

        var apiControllers = SortedHttpRequestHandlers.Where(x => x is ApiController);
        // ReSharper disable once PossibleMultipleEnumeration
        var duplicateApiControllers = apiControllers.GroupBy(x => x.GetType().Name).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
        foreach (var duplicateApiController in duplicateApiControllers)
        {
            Console.WriteLine($"WARNING! Found multiple api controllers named {duplicateApiController}:");
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (var duplicate in apiControllers.Where(x => x.GetType().Name == duplicateApiController))
            {
                Console.WriteLine($" - {duplicate.GetType().FullName}");
            }
        }

        if (Debugger.IsAttached) ShowErrorDetails = true;
    }
    protected virtual ImmutableList<IWebMonkStartupScript> GetAndSortWebMonkStartupScripts(Assembly[] httpRequestHandlerAssemblies)
    {
        var startupScripts = new List<IWebMonkStartupScript>();
            
        //Add non-abstract classes that implement IHttpRequestHandler
        foreach (var assembly in httpRequestHandlerAssemblies)
        {
            //We specifically skip Microsoft.Data.SqlClient assembly because of the
            //problem in .net 8.0. See https://github.com/dotnet/runtime/issues/86969
            //If you ever change this, search solution for 23ec7fc2d6eaa4a5 (PublicKeyToken)
            if (assembly.FullName == "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5") continue;
            
            var typesImplementingIWebMonkStartupScript = assembly
                .GetTypes()
                .Where(x => x.IsClass && 
                            !x.IsAbstract && 
                            x.GetConstructor(Type.EmptyTypes) != null && 
                            typeof(IWebMonkStartupScript).IsAssignableFrom(x)).ToList();

            foreach (var type in typesImplementingIWebMonkStartupScript)
            {
                var startupScript = (IWebMonkStartupScript)Activator.CreateInstance(type, null);
                startupScripts.Add(startupScript);
            }
        }            
            
        //Order by Priority
        var sortedStartupScripts = startupScripts.OrderBy(x => x.Priority).ToImmutableList();

        return sortedStartupScripts;
    }
    protected virtual ImmutableList<IHttpRequestHandler> GetAndSortHttpRequestHandlers(Assembly[] httpRequestHandlerAssemblies)
    {
        var httpRequestHandlers = new List<IHttpRequestHandler>();
            
        //Add non-abstract classes that implement IHttpRequestHandler
        foreach (var assembly in httpRequestHandlerAssemblies)
        {
            //We specifically skip Microsoft.Data.SqlClient assembly because of the
            //problem in .net 8.0. See https://github.com/dotnet/runtime/issues/86969
            //If you ever change this, search solution for 23ec7fc2d6eaa4a5 (PublicKeyToken)
            if (assembly.FullName == "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5") continue;
            
            var typesImplementingIHttpRequestHandler = assembly
                .GetTypes()
                .Where(x => x.IsClass && 
                            !x.IsAbstract && 
                            x.GetConstructor(Type.EmptyTypes) != null &&
                            typeof(IHttpRequestHandler).IsAssignableFrom(x)).ToList();

            foreach (var type in typesImplementingIHttpRequestHandler)
            {
                var httpRequestHandler = (IHttpRequestHandler)Activator.CreateInstance(type, null);
                httpRequestHandlers.Add(httpRequestHandler);
            }
        }            
            
        //Order by Priority
        var sortedHttpRequestHandlers = httpRequestHandlers.OrderBy(x => x.Priority).ToImmutableList();

        return sortedHttpRequestHandlers;
    }
    #endregion 
        
    #region Methods
    public static void OpenInBrowser(string url)
    {
        #pragma warning disable 4014
        Task.Run(async () =>
        {
            // ReSharper disable once MethodSupportsCancellation
            await Task.Delay(1000).ConfigureAwait(false);
            new Process
            {
                StartInfo =
                {
                    UseShellExecute = true, 
                    FileName = url
                }
            }.Start();
        });
        #pragma warning restore 4014
    }
        
    public Task RunAsync(string? localPathUrl = null, bool autoregisterWithNetsh = true, bool autoUnregisterNetsh = true)
    {
        return RunAsync(CancellationToken.None, localPathUrl, autoregisterWithNetsh, autoUnregisterNetsh);
    }
    public virtual async Task RunAsync(CancellationToken cancellationToken, string? localPathUrl = null, bool autoregisterWithNetsh = true, bool autoUnregisterNetsh = true)
    {
        //Run all the startup scripts
        foreach (var startupScript in GetAndSortWebMonkStartupScripts(AppAssemblies)) await startupScript.ExecuteStartupTaskAsync().ConfigureAwait(false);
            
        try
        {
            StartListener(autoregisterWithNetsh);
            if (localPathUrl != null) 
            {
                // ReSharper disable once RedundantAssignment
                if (localPathUrl.Trim().StartsWith("/")) localPathUrl = localPathUrl.Trim()[1..];
                //OpenInBrowser($"{NavigatingBaseUrl}{localPathUrl}");
            }
            await RunListenerLoopAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (autoUnregisterNetsh) UnregisterWithNetsh();
        }
    }
    public virtual void StopRun()
    {
        HttpListener.Stop();
    }

    public virtual void RegisterWithNetsh()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RunNetsh($"http add urlacl url={ListeningBaseUrl} user=everyone");
            Console.WriteLine($"{ListeningBaseUrl} registered successfully with Netsh...");
        }
    }
    public virtual void UnregisterWithNetsh()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                RunNetsh($"http delete urlacl url={ListeningBaseUrl}");
                Console.WriteLine($"{ListeningBaseUrl} unregistered successfully with Netsh...");
            }
            catch (SystemException ex)
            { 
                Console.WriteLine($"Unable to unregister {ListeningBaseUrl}. Error: {ex.Message}");
            }
        }
    }

    public static int FindFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        try
        {
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }
        finally
        {
            listener.Stop();
        }
    }
    #endregion

    #region Protected Methods
    protected virtual void StartListener(bool autoregisterListeningBaseUrlWithNetsh)
    {
        //Start Listener (register with nestsh if needed)
        try
        {
            HttpListener.Start();
        }
        catch (HttpListenerException ex)
        {
            //exception means user most likely aborted
            if (ex.Message == "Access is denied." && autoregisterListeningBaseUrlWithNetsh) 
            { 
                HttpListener = new HttpListener 
                { 
                    AuthenticationSchemes = AuthenticationSchemes.Anonymous,
                    Prefixes = { ListeningBaseUrl }
                };
                RegisterWithNetsh();
                HttpListener.Start();
            }
            else
            {
                throw;
            }
        }
        Console.WriteLine(Prompt);
    }
    protected virtual async Task RunListenerLoopAsync(CancellationToken cancellationToken)
    {
        //Start the session state "garbage collector." This method never returns and just keeps running in the background
        #pragma warning disable 4014
        SessionState.RemoveExpiredTasksServiceAsync(SessionTimeoutMinutes, cancellationToken);    
        #pragma warning restore 4014
            
        while (true)
        {
            Task<HttpListenerContext> httpContextTask;
            try 
            { 
                httpContextTask = HttpListener.GetContextAsync(); 

                var tcs = new TaskCompletionSource<bool>();
                await using (cancellationToken.Register(x => ((TaskCompletionSource<bool>)x).TrySetResult(true), tcs))
                {
                    if (httpContextTask != await Task.WhenAny(httpContextTask, tcs.Task).ConfigureAwait(false)) 
                    {
                        StopRun();
                        break;
                    }
                }
            }
            catch (HttpListenerException ex) 
            { 
                //exception means user most likely aborted
                if (ex.Message == "The I/O operation has been aborted because of either a thread exit or an application request.") break; 
                throw;
            } 
            
            var httpListenerContext = await httpContextTask.ConfigureAwait(false);
            if (httpListenerContext == null) continue;
            if (httpListenerContext.Request == null)
            {
                //400 Bad Request
                try
                {
                    var response = httpListenerContext.Response;
                    var statusCode = HttpStatusCode.BadRequest;
                    httpListenerContext.Response.StatusCode = (int)statusCode;
                    var description = ShowErrorDetails ? "httpListenerContext.Request == null" : null;
                    var bytes = Encoding.Default.GetBytes(GetErrorHtmlPage(statusCode, description));
                    await response.OutputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);

                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception) {} //ignore exceptions

                try { httpListenerContext.Response.Close(); }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception) {} //ignore exceptions

                continue;
            }
            var httpListenerContextWrapper = new HttpListenerContextWrapper(httpListenerContext);
            #pragma warning disable 4014
            ProcessHttpRequestAsync(httpListenerContextWrapper, cancellationToken);
            #pragma warning restore 4014
        }
    }

    //override this method to use different managers
    protected virtual HttpContext CreateHttpContext(IHttpListenerContext httpListenerContext)
    {
        var routeManager = new RouteManager(httpListenerContext.Request.HttpMethod, NavigatingBaseUrl, httpListenerContext.Request.Url.LocalPath, httpListenerContext.Request.Url.Query);
        var prefixManager = new PrefixManager();
        var valueProviderManager = new ValueProviderManager(httpListenerContext);
        var staticModelBinderManager = new StaticModelBinderManager();
        var sessionId = SessionState.ManageSessionCookie(httpListenerContext);
        var sessionState = SessionState.GetOrCreate(sessionId);

        var httpContext = new HttpContext(this, httpListenerContext, routeManager, prefixManager, valueProviderManager, staticModelBinderManager, sessionState);

        return httpContext;
    }

    public virtual async Task ProcessHttpRequestAsync(IHttpListenerContext httpListenerContext, CancellationToken cancellationToken)
    {
        var httpContext = CreateHttpContext(httpListenerContext);
        using(new HttpContextScope(httpContext))
        {
            try
            {
                await ValidateSessionIdAsync(HttpContext.Current.SessionId).ConfigureAwait(false);
                await UpdateHttpMethodIfOverridenAsync().ConfigureAwait(false);
                    
                IHttpRequestHandler.HttpRequestHandlerResult? result = null;
                foreach (var httpRequestHandler in SortedHttpRequestHandlers)
                {
                    result = await httpRequestHandler.TryExecuteHttpRequestAsync(cancellationToken).ConfigureAwait(false);
                    if (result.Success) 
                    {
                        if (httpRequestHandler.SaveSessionState && !HttpContext.Current.SessionState.IsBlank) HttpContext.Current.SessionState.SaveSessionState();
                        await result.ExecuteResultAsync().ConfigureAwait(false);
                        break;
                    }
                }
                if (result == null || !result.Success)
                {
                    //404 Page Not Found Error
                    await new StatusCodeResult(HttpStatusCode.NotFound).ExecuteResultAsync().ConfigureAwait(false);
                    await OnPageNotFoundAsync().ConfigureAwait(false);
                }
            }
            catch (Exception404PageNotFound)
            {
                //404 Page Not Found Error
                await new StatusCodeResult(HttpStatusCode.NotFound).ExecuteResultAsync().ConfigureAwait(false);
                await OnPageNotFoundAsync().ConfigureAwait(false);
            }
            catch (Exception415UnsupportedMediaType ex)
            {
                //415 Unsupported Media Type (typically this is used for bad Content-Type)
                await new StatusCodeResult(HttpStatusCode.UnsupportedMediaType).ExecuteResultAsync().ConfigureAwait(false);
                await OnUnsupportedMediaTypeAsync(ex).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //500 Internal Server Error
                await new StatusCodeResult(HttpStatusCode.InternalServerError, ex.ToString()).ExecuteResultAsync().ConfigureAwait(false);
                await OnInternalServerErrorAsync(ex).ConfigureAwait(false);
            }
            finally
            {
                httpListenerContext.Response.Close();
            }
        }
    }       
    protected virtual async Task UpdateHttpMethodIfOverridenAsync()
    {
        var valueProviders = await HttpContext.Current.ValueProviderManager.GetValueProvidersListAsync().ConfigureAwait(false);
        var httpMethodOverrideResult = valueProviders.GetValueOrDefault<string?>("X-HTTP-Method-Override");
        HttpContext.Current.RouteManager.OverridenHttpMethod = httpMethodOverrideResult.UpdateInternal(HttpContext.Current.RouteManager.OverridenHttpMethod);
    }
    protected virtual void RunNetsh(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            Verb = "runas"
        };

        try
        {
            var process = Process.Start(startInfo);
            if (process == null) throw new Exception("Netsh process after starting is null");
            process.WaitForExit();
            if (process.ExitCode != 0) throw new Exception($"returned exit code {process.ExitCode}");
        }
        catch (Win32Exception ex1)
        {
            const int errorCancelled = 1223; //The operation was canceled by the user.
            if (ex1.NativeErrorCode == errorCancelled) throw new WebMonkException("You must allow Administrator access in order to register web projects with netsh.");
            throw;
        }
        catch (Exception ex2)
        {
            throw new WebMonkException(ex2.Message);
        }
    }

    public virtual string GetErrorHtmlPage(HttpStatusCode statusCode, string? additionalDescription = null)
    {
        if (additionalDescription == null) additionalDescription = "";
        else additionalDescription = $"\n\n{additionalDescription}";
            
        return $"Error {(int)statusCode}: {statusCode.ToString().InsertSpacesBetweenWords()}{additionalDescription}";
    }

    protected virtual bool IsValidSessionId(string sessionId)
    {
        if (sessionId.Length != 177) return false;

        if (int.TryParse(sessionId[..2], NumberStyles.HexNumber, null, out var index))
        {
            if (int.TryParse(sessionId[2..4], NumberStyles.HexNumber, null, out var chrIndex))
            {
                var chr = (char)('a' + chrIndex);
                if (sessionId.Length - 1 < index + 4) return false;
                var chrInSessionId = sessionId[index + 4];
                return chrInSessionId == chr;
            }
        }
        return false;
    }
    protected virtual Task ValidateSessionIdAsync(string sessionId)
    {
        if (!IsValidSessionId(sessionId)) throw new WebMonkException($"Invalid SessionId: '{sessionId}'");
        return Task.CompletedTask;
    }

    protected virtual Task OnInternalServerErrorAsync(Exception ex)
    {
        //override this if you want to log/email a server error
        return Task.CompletedTask;
    }
    protected virtual Task OnPageNotFoundAsync()
    {
        //override this if you want to log/email a server error
        return Task.CompletedTask;
    }
    protected virtual Task OnUnsupportedMediaTypeAsync(Exception415UnsupportedMediaType ex)
    {
        //override this if you want to log/email a server error
        return Task.CompletedTask;
    }
    #endregion

    #region Properties
    public Assembly[] AppAssemblies { get ; }
    public bool ShowErrorDetails { get; set; } 
    public int SessionTimeoutMinutes { get; set; } = 20;
    public string? LoginUrl { get; set; }
    public ConcurrentBag<ActionFilterAttribute> GlobalFilters { get; } = new();
    public IMvcLayout? DefaultLayout { get; set; }

    private ImmutableList<IHttpRequestHandler> SortedHttpRequestHandlers { get; }
        
    public const string EOL = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker

    protected HttpListener HttpListener { get; set; }
    public string ListeningBaseUrl { get; }
    public string NavigatingBaseUrl { get; }
        
    public string Prompt {get; private set; }
    #endregion
}