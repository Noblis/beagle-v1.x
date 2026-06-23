#nullable disable

using System.Net;
using System.Net.Http.Headers;

namespace Supermodel.Client.Backend.Multipart;

public class HttpUnsortedResponse
{
    public HttpUnsortedResponse()
    {
        // Collection of unsorted headers. Later we will sort it into the appropriate
        // HttpContentHeaders, HttpRequestHeaders, and HttpResponseHeaders.
        HttpHeaders = new HttpUnsortedHeaders();
    }

    public Version Version { get; set; }

    public HttpStatusCode StatusCode { get; set; }

    public string ReasonPhrase { get; set; }

    public HttpHeaders HttpHeaders { get; private set; }
}