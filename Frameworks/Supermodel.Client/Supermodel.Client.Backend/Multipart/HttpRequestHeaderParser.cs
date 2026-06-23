#nullable disable

namespace Supermodel.Client.Backend.Multipart;

public class HttpRequestHeaderParser
{
    internal const int DefaultMaxRequestLineSize = 2 * 1024;
    internal const int DefaultMaxHeaderSize = 16 * 1024; // Same default size as IIS has for regular requests

    private HttpRequestState _requestStatus = HttpRequestState.RequestLine;

    private readonly HttpRequestLineParser _requestLineParser;
    private readonly InternetMessageFormatHeaderParser _headerParser;

    public HttpRequestHeaderParser(HttpUnsortedRequest httpRequest, int maxRequestLineSize = DefaultMaxRequestLineSize, int maxHeaderSize = DefaultMaxHeaderSize)
    {
        if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

        HttpUnsortedRequest httpRequest1 = httpRequest;

        // Create request line parser
        _requestLineParser = new HttpRequestLineParser(httpRequest1, maxRequestLineSize);

        // Create header parser
        _headerParser = new InternetMessageFormatHeaderParser(httpRequest1.HttpHeaders, maxHeaderSize);
    }

    private enum HttpRequestState
    {
        RequestLine = 0, // parsing request line
        RequestHeaders // reading headers
    }

    public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        var parseStatus = ParserState.NeedMoreData;
        ParserState subParseStatus;

        switch (_requestStatus)
        {
            case HttpRequestState.RequestLine:
                try
                {
                    subParseStatus = _requestLineParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
                }
                catch (Exception)
                {
                    subParseStatus = ParserState.Invalid;
                }

                if (subParseStatus == ParserState.Done)
                {
                    _requestStatus = HttpRequestState.RequestHeaders;
                    goto case HttpRequestState.RequestHeaders;
                }
                else if (subParseStatus != ParserState.NeedMoreData)
                {
                    // Report error - either Invalid or DataTooBig
                    parseStatus = subParseStatus;
                    // ReSharper disable once RedundantJumpStatement
                    break;
                }

                break; // read more data

            case HttpRequestState.RequestHeaders:
                if (bytesConsumed >= bytesReady) break; // we already can tell we need more data
                try
                {
                    subParseStatus = _headerParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
                }
                catch (Exception)
                {
                    subParseStatus = ParserState.Invalid;
                }

                if (subParseStatus == ParserState.Done) parseStatus = subParseStatus;
                else if (subParseStatus != ParserState.NeedMoreData) parseStatus = subParseStatus;

                break; // need more data
        }

        return parseStatus;
    }
}