
using System;

namespace WebMonk.Multipart;

public class HttpResponseHeaderParser
{
    public const int DefaultMaxStatusLineSize = 2 * 1024;
    public const int DefaultMaxHeaderSize = 16 * 1024; // Same default size as IIS has for HTTP requests

    private HttpResponseState _responseStatus = HttpResponseState.StatusLine;

    private readonly HttpStatusLineParser _statusLineParser;
    private readonly InternetMessageFormatHeaderParser _headerParser;

    public HttpResponseHeaderParser(HttpUnsortedResponse httpResponse, int maxResponseLineSize = DefaultMaxStatusLineSize, int maxHeaderSize = DefaultMaxHeaderSize)
    {
        if (httpResponse == null) throw new ArgumentNullException(nameof(httpResponse));

        HttpUnsortedResponse httpResponse1 = httpResponse;

        // Create status line parser
        _statusLineParser = new HttpStatusLineParser(httpResponse1, maxResponseLineSize);

        // Create header parser
        _headerParser = new InternetMessageFormatHeaderParser(httpResponse1.HttpHeaders, maxHeaderSize);
    }

    private enum HttpResponseState
    {
        StatusLine = 0, // parsing status line
        ResponseHeaders // reading headers
    }

    public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        var parseStatus = ParserState.NeedMoreData;
        ParserState subParseStatus;

        switch (_responseStatus)
        {
            case HttpResponseState.StatusLine:
                try
                {
                    subParseStatus = _statusLineParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
                }
                catch (Exception)
                {
                    subParseStatus = ParserState.Invalid;
                }

                if (subParseStatus == ParserState.Done)
                {
                    _responseStatus = HttpResponseState.ResponseHeaders;
                    goto case HttpResponseState.ResponseHeaders;
                }
                else if (subParseStatus != ParserState.NeedMoreData)
                {
                    // Report error - either Invalid or DataTooBig
                    parseStatus = subParseStatus;
                }

                break; // read more data

            case HttpResponseState.ResponseHeaders:
                if (bytesConsumed >= bytesReady) break;  // we already can tell we need more data

                try
                {
                    subParseStatus = _headerParser.ParseBuffer(buffer, bytesReady, ref bytesConsumed);
                }
                catch (Exception)
                {
                    subParseStatus = ParserState.Invalid;
                }

                if (subParseStatus == ParserState.Done)
                {
                    parseStatus = subParseStatus;
                }
                else if (subParseStatus != ParserState.NeedMoreData)
                {
                    parseStatus = subParseStatus;
                    // ReSharper disable once RedundantJumpStatement
                    break;
                }

                break; // need more data
        }

        return parseStatus;
    }
}