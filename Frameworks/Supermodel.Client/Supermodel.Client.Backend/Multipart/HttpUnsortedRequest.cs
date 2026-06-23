#nullable disable

using System.Net.Http.Headers;

namespace Supermodel.Client.Backend.Multipart;

public class HttpUnsortedRequest
{
    public HttpUnsortedRequest()
    {
        // Collection of unsorted headers. Later we will sort it into the appropriate
        // HttpContentHeaders, HttpRequestHeaders, and HttpResponseHeaders.
        HttpHeaders = new HttpUnsortedHeaders();
    }

    public HttpMethod Method { get; set; }

    public string RequestUri { get; set; }

    public Version Version { get; set; }

    public HttpHeaders HttpHeaders { get; private set; }
}