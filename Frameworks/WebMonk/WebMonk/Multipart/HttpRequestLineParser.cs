using System;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text;

namespace WebMonk.Multipart;

public class HttpRequestLineParser
{
    public const int MinRequestLineSize = 14;
    private const int DefaultTokenAllocation = 2 * 1024;

    private int _totalBytesConsumed;
    private readonly int _maximumHeaderLength;

    private HttpRequestLineState _requestLineState;
    private HttpUnsortedRequest _httpRequest;
    private readonly StringBuilder _currentToken = new(DefaultTokenAllocation);

    public HttpRequestLineParser(HttpUnsortedRequest httpRequest, int maxRequestLineSize)
    {
        // The minimum length which would be an empty header terminated by CRLF
        if (maxRequestLineSize < MinRequestLineSize) throw new ArgumentOutOfRangeException(nameof(maxRequestLineSize));
        if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

        _httpRequest = httpRequest;
        _maximumHeaderLength = maxRequestLineSize;
    }

    private enum HttpRequestLineState
    {
        RequestMethod = 0,
        RequestUri,
        BeforeVersionNumbers,
        MajorVersionNumber,
        MinorVersionNumber,
        AfterCarriageReturn
    }

    public ParserState ParseBuffer(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        ParserState parseStatus = ParserState.NeedMoreData;

        if (bytesConsumed >= bytesReady) return parseStatus;  // We already can tell we need more data

        try
        {
            parseStatus = ParseRequestLine(buffer, bytesReady, ref bytesConsumed, ref _requestLineState, _maximumHeaderLength, ref _totalBytesConsumed, _currentToken, _httpRequest);
        }
        catch (Exception)
        {
            parseStatus = ParserState.Invalid;
        }

        return parseStatus;
    }

    private static ParserState ParseRequestLine(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed,
        ref HttpRequestLineState requestLineState,
        int maximumHeaderLength,
        ref int totalBytesConsumed,
        StringBuilder currentToken,
        HttpUnsortedRequest httpRequest)
    {
        Contract.Assert((bytesReady - bytesConsumed) >= 0, "ParseRequestLine()|(bytesReady - bytesConsumed) < 0");
        Contract.Assert(maximumHeaderLength <= 0 || totalBytesConsumed <= maximumHeaderLength, "ParseRequestLine()|Headers already read exceeds limit.");

        // Remember where we started.
        var initialBytesParsed = bytesConsumed;
        int segmentStart;

        // Set up parsing status with what will happen if we exceed the buffer.
        var parseStatus = ParserState.DataTooBig;
        var effectiveMax = maximumHeaderLength <= 0 ? int.MaxValue : (maximumHeaderLength - totalBytesConsumed + bytesConsumed);
        if (bytesReady < effectiveMax)
        {
            parseStatus = ParserState.NeedMoreData;
            effectiveMax = bytesReady;
        }

        Contract.Assert(bytesConsumed < effectiveMax, "We have already consumed more than the max header length.");

        switch (requestLineState)
        {
            case HttpRequestLineState.RequestMethod:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != ' ')
                {
                    if (buffer[bytesConsumed] < 0x21 || buffer[bytesConsumed] > 0x7a)
                    {
                        parseStatus = ParserState.Invalid;
                        goto quit;
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        var method = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(method);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    var method = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(method);
                }

                // Copy value out
                httpRequest.Method = new HttpMethod(currentToken.ToString());
                currentToken.Clear();

                // Move past the SP
                requestLineState = HttpRequestLineState.RequestUri;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HttpRequestLineState.RequestUri;

            case HttpRequestLineState.RequestUri:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != ' ')
                {
                    if (buffer[bytesConsumed] == '\r')
                    {
                        parseStatus = ParserState.Invalid;
                        goto quit;
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        string addr = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(addr);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    string addr = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(addr);
                }

                // URI validation happens when we create the URI later.
                if (currentToken.Length == 0) throw new FormatException("HttpMessageParserEmptyUri");

                // Copy value out
                httpRequest.RequestUri = currentToken.ToString();
                currentToken.Clear();

                // Move past the SP
                requestLineState = HttpRequestLineState.BeforeVersionNumbers;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case HttpRequestLineState.BeforeVersionNumbers;

            case HttpRequestLineState.BeforeVersionNumbers:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != '/')
                {
                    if (buffer[bytesConsumed] < 0x21 || buffer[bytesConsumed] > 0x7a)
                    {
                        parseStatus = ParserState.Invalid;
                        goto quit;
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        string token = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(token);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    string token = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(token);
                }

                // Validate value
                var version = currentToken.ToString();
                if (string.CompareOrdinal(FormattingUtilities.HttpVersionToken, version) != 0) throw new FormatException("HttpInvalidVersion");
                currentToken.Clear();

                // Move past the '/'
                requestLineState = HttpRequestLineState.MajorVersionNumber;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HttpRequestLineState.MajorVersionNumber;

            case HttpRequestLineState.MajorVersionNumber:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != '.')
                {
                    if (buffer[bytesConsumed] < '0' || buffer[bytesConsumed] > '9')
                    {
                        parseStatus = ParserState.Invalid;
                        goto quit;
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        string major = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(major);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    var major = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(major);
                }

                // Move past the "."
                currentToken.Append('.');
                requestLineState = HttpRequestLineState.MinorVersionNumber;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case HttpRequestLineState.MinorVersionNumber;

            case HttpRequestLineState.MinorVersionNumber:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != '\r')
                {
                    if (buffer[bytesConsumed] < '0' || buffer[bytesConsumed] > '9')
                    {
                        parseStatus = ParserState.Invalid;
                        goto quit;
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        string minor = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(minor);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    string minor = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(minor);
                }

                // Copy out value
                httpRequest.Version = Version.Parse(currentToken.ToString());
                currentToken.Clear();

                // Move past the CR
                requestLineState = HttpRequestLineState.AfterCarriageReturn;
                if (++bytesConsumed == effectiveMax) goto quit;
                goto case HttpRequestLineState.AfterCarriageReturn;

            case HttpRequestLineState.AfterCarriageReturn:
                if (buffer[bytesConsumed] != '\n')
                {
                    parseStatus = ParserState.Invalid;
                    goto quit;
                }

                parseStatus = ParserState.Done;
                bytesConsumed++;
                break;
        }

        quit:
        totalBytesConsumed += bytesConsumed - initialBytesParsed;
        return parseStatus;
    }
}