using System.Net;
using System.Threading.Tasks;
using WebMonk.Context;

namespace WebMonk.Results;

public class RedirectResult : ActionResult
{
    #region Constructors
    public RedirectResult(string redirectLocation)
    {
        RedirectLocation = redirectLocation;
    }
    #endregion
        
    #region Overrides
    public override async Task ExecuteResultAsync()
    {
        //We do this because we want to preserve TempData through multiple redirects
        HttpContext.Current.TempData.MergeCurrentIntoFuture(HttpContext.Current.SessionId);
            
        var response = HttpContext.Current.HttpListenerContext.Response;

        response.StatusCode = (int)HttpStatusCode.Redirect;
        response.RedirectLocation = RedirectLocation;
        await response.OutputStream.WriteAsync([], 0, 0).ConfigureAwait(false);
    }
    #endregion

    #region Properties
    public string RedirectLocation { get; }
    #endregion
}