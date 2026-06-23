using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Supermodel.Persistence.DataContext;
using Supermodel.Persistence.UnitOfWork;
using WebMonk.Context;
using WebMonk.Context.WMHttpListenerObjects;
using WebMonk.HttpRequestHandlers;
using WebMonk.Multipart;

namespace Supermodel.Presentation.WebMonk.Batch;

public abstract class BatchHttpRequestHandlerBase<TDataContext> : IHttpRequestHandler where TDataContext : class, IDataContext, new()
{
    #region IHttpRequestHandler implementation
    public async Task<IHttpRequestHandler.HttpRequestHandlerResult> TryExecuteHttpRequestAsync(CancellationToken cancellationToken)
    {
        var request = HttpContext.Current.HttpListenerContext.Request;

        var httpMethod = request.HttpMethod;
        var localParts = HttpContext.Current.RouteManager.LocalPathParts;
        if (httpMethod != "POST" || localParts.Length != 2 || localParts[0] != "api" || localParts[1] != "$batch") return IHttpRequestHandler.HttpRequestHandlerResult.False;
            
        //We specifically are using not the overriden HttpMethod but the real one here
        if (!request.HasEntityBody)
        {
            return new IHttpRequestHandler.HttpRequestHandlerResult(true, async () => 
            { 
                var response = HttpContext.Current.HttpListenerContext.Response;
                response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                await response.OutputStream.WriteAsync([], 0, 0, cancellationToken).ConfigureAwait(false);
            });
        }

        var streamContent = new StreamContent(request.InputStream);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);

        var provider = await streamContent.ReadAsMultipartAsync(cancellationToken).ConfigureAwait(false);
            
        var batchResponse = HttpContext.Current.HttpListenerContext.Response;
        batchResponse.StatusCode = (int)HttpStatusCode.OK;
            
        var responses = new List<IHttpListenerResponse>();
            
        var unitOfWork = new UnitOfWork<TDataContext>();
        using(var transaction = unitOfWork.Context.BeginTransaction())
        {
            var rollback = false;
            await using (unitOfWork)
            {
                foreach (var httpContent in provider.Contents)
                {
                    await using (var stream = await httpContent.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    {
                        using (var streamReader = new StreamReader(stream, request.ContentEncoding))
                        {
                            try
                            {
                                var httpRequestRaw = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                                var httpListenerContext = new BatchHttpListenerContext(HttpContext.Current.HttpListenerContext, httpRequestRaw);

                                await HttpContext.Current.WebServer.ProcessHttpRequestAsync(httpListenerContext, CancellationToken.None).ConfigureAwait(false);
                                if (httpListenerContext.Response.StatusCode < 200 || httpListenerContext.Response.StatusCode > 299)
                                {
                                    unitOfWork.Context.CommitOnDispose = false;
                                    rollback = true;
                                }
                                responses.Add(httpListenerContext.Response);
                            }
                            catch (Exception)
                            {
                                unitOfWork.Context.CommitOnDispose = false;
                                rollback = true;
                            }
                        }
                    }
                }

                if (unitOfWork.Context.CommitOnDispose && !rollback)
                {
                    await unitOfWork.Context.FinalSaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }                
            }
        }

        return new IHttpRequestHandler.HttpRequestHandlerResult(true, async () => 
        { 
            batchResponse.Headers.Add("Transfer-Encoding", "chunked");
            var boundary = Guid.NewGuid().ToString();
            batchResponse.ContentType = $@"multipart/batch; boundary=""{boundary}""";
            var boundaryStr = "\r\n--" + boundary + "\r\n";
            var endBoundaryStr = "\r\n--" + boundary + "--";
            var sb = new StringBuilder();
            foreach (var response in responses)
            {
                string body;
                response.OutputStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(response.OutputStream))
                {
                    body = await streamReader.ReadToEndAsync(cancellationToken);
                }

                sb.Append(boundaryStr);
                sb.Append("Content-Type: application/http; msgtype=response\r\n\r\n");
                sb.Append($"HTTP/{batchResponse.ProtocolVersion.Major}.{batchResponse.ProtocolVersion.Minor} {response.StatusCode} {response.StatusDescription}\r\n");

                //sb.Append(response.ProtocolVersion.ToString());
                foreach (string? headerKey in response.Headers.Keys)
                {
                    if (headerKey != null) 
                    {
                        sb.Append(headerKey + ": " + string.Join(", ", response.Headers[headerKey]) + "\r\n");                        
                    }
                }
                if (!string.IsNullOrEmpty(body)) sb.Append("Content-Length: " + body.Length + "\r\n"); 

                sb.Append("\r\n");
                sb.Append(body);
            }
            sb.Append(endBoundaryStr);
            var subContent = sb.ToString();
            var bytes = Encoding.ASCII.GetBytes(subContent);
            await batchResponse.OutputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        });
    }

    public virtual int Priority => 500;
    public virtual bool SaveSessionState => false;
    #endregion

    #region Helper Methods
    protected static Task EncodeStringToStreamAsync(Stream stream, string input)
    {
        var bytes = Encoding.GetEncoding(28591).GetBytes(input);
        return stream.WriteAsync(bytes, 0, bytes.Length);
    }
    #endregion
}