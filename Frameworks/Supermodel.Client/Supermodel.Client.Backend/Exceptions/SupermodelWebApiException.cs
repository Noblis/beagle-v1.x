using System.Net;
using Newtonsoft.Json;
using Supermodel.DataAnnotations.Exceptions;

namespace Supermodel.Client.Backend.Exceptions;

public class SupermodelWebApiException : SupermodelException
{
    #region Embedded Types
    // ReSharper disable once ClassNeverInstantiated.Local
    private class JsonMessage
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string? Message { get; set; }
    }
    #endregion

    public SupermodelWebApiException(HttpStatusCode statusCode, string content) : base((int)statusCode + ":" + statusCode + ". Content: " + content)
    {
        StatusCode = statusCode;
        Content = content;
    }

    public HttpStatusCode StatusCode { get; }
    public string Content { get; }
    public string? ContentJsonMessage => JsonConvert.DeserializeObject<JsonMessage>(Content)!.Message;
}