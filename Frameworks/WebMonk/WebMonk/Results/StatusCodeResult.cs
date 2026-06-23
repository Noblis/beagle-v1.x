using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebMonk.Context;

namespace WebMonk.Results;

public class StatusCodeResult : ActionResult
{
    #region Constructors
    public StatusCodeResult(HttpStatusCode statusCode, string? description = null)
    {
        Description = description;
        StatusCode = statusCode;
    }
    #endregion

    #region Methods
    public override async Task ExecuteResultAsync()
    {
        var response = HttpContext.Current.HttpListenerContext.Response;
        var httpListenerContext = HttpContext.Current.HttpListenerContext;
        var webServer = HttpContext.Current.WebServer;
            
        httpListenerContext.Response.StatusCode = (int)StatusCode;
        if (httpListenerContext.Response.StatusCode < 200 || httpListenerContext.Response.StatusCode > 299)
        {
            var description = webServer.ShowErrorDetails ? Description : null;
            var bytes = Encoding.Default.GetBytes(webServer.GetErrorHtmlPage(StatusCode, description));
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }
        else
        {
            await response.OutputStream.WriteAsync([], 0, 0).ConfigureAwait(false);
        }
    }
    #endregion

    #region Properties
    public HttpStatusCode StatusCode { get; }
    public string? Description { get; } 
    #endregion
}