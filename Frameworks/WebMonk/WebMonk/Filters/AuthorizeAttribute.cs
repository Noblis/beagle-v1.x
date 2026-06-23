using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebMonk.Context;
using WebMonk.Filters.Base;
using WebMonk.HttpRequestHandlers.Controllers;

namespace WebMonk.Filters;

public class AuthorizeAttribute : ActionFilterAttribute
{
    #region Constructors
    public AuthorizeAttribute(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
    #endregion
        
    #region Overrides
    public override Task<ActionFilterResult> BeforeActionAsync(ActionFilterContext filterContext)
    {
        if (HttpContext.Current.Claims.Any(x => x.Type == ClaimTypes.NameIdentifier))
        {
            var roleClaims = HttpContext.Current.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            if (AllowedRoles.Length == 0 || AllowedRoles.Any(x => roleClaims.Contains(x))) return Task.FromResult(ActionFilterResult.Proceed);

            return Task.FromResult(new ActionFilterResult(true, true, async ()=>
            { 
                var statusCode = HttpStatusCode.Forbidden;
                HttpContext.Current.HttpListenerContext.Response.StatusCode = (int)statusCode;
                var errorBytes = Encoding.Default.GetBytes(HttpContext.Current.WebServer.GetErrorHtmlPage(statusCode));
                await HttpContext.Current.HttpListenerContext.Response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length, CancellationToken.None).ConfigureAwait(false);
            }));
        }
        else
        {
            //if we are running an api controller or login page is undefined, return unauthorized, otherwise redirect user to the login page
            if (filterContext.Controller is MvcController && !string.IsNullOrEmpty(HttpContext.Current.WebServer.LoginUrl))
            {
                var loginUrl = HttpContext.Current.WebServer.LoginUrl.Trim();
                var routeManager = HttpContext.Current.RouteManager;
                loginUrl = $"{loginUrl}?ReturnUrl={HttpUtility.UrlEncode(routeManager.LocalPathWithQueryString)}";

                return Task.FromResult(new ActionFilterResult(true, true, async ()=>
                { 
                    //We do this because we want to preserve TempData through multiple redirects
                    HttpContext.Current.TempData.MergeCurrentIntoFuture(HttpContext.Current.SessionId);

                    var response = HttpContext.Current.HttpListenerContext.Response;
                    response.StatusCode = (int)HttpStatusCode.Redirect;
                    response.RedirectLocation = loginUrl;
                    await response.OutputStream.WriteAsync([], 0, 0).ConfigureAwait(false);
                }));
            }
            else
            {
                return Task.FromResult(new ActionFilterResult(true, true, async ()=>
                { 
                    var statusCode = HttpStatusCode.Unauthorized;
                    HttpContext.Current.HttpListenerContext.Response.StatusCode = (int)statusCode;
                    var errorBytes = Encoding.Default.GetBytes(HttpContext.Current.WebServer.GetErrorHtmlPage(statusCode));
                    await HttpContext.Current.HttpListenerContext.Response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length, CancellationToken.None).ConfigureAwait(false);
                }));
            }
        }
    }
    public override int Priority => -100;
    #endregion

    #region Properties
    protected string[] AllowedRoles { get; }
    #endregion
}