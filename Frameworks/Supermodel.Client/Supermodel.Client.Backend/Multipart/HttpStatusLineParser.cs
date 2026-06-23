#nullable disable

using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Text;

namespace Supermodel.Client.Backend.Multipart;

public class HttpStatusLineParser
{
    public const int MinStatusLineSize = 15;
    private const int DefaultTokenAllocation = 2 * 1024;
    //private const int MaxStatusCode = 1000;

    private int _totalBytesConsumed;
    private readonly int _maximumHeaderLength;

    private HttpStatusLineState _statusLineState;
    private HttpUnsortedResponse _httpResponse;
    private readonly StringBuilder _currentToken = new(DefaultTokenAllocation);

    public HttpStatusLineParser(HttpUnsortedResponse httpResponse, int maxStatusLineSize)
    {
        // The minimum length which would be an empty header terminated by CRLF
        if (maxStatusLineSize < MinStatusLineSize) throw new ArgumentOutOfRangeException(nameof(maxStatusLineSize));
        if (httpResponse == null) throw new ArgumentNullException(nameof(httpResponse));

        _httpResponse = httpResponse;
        _maximumHeaderLength = maxStatusLineSize;
    }

    private enum HttpStatusLineState
    {
        BeforeVersionNumbers = 0,
        MajorVersionNumber,
        MinorVersionNumber,
        StatusCode,
        ReasonPhrase,
        AfterCarriageReturn
    }

    public ParserState ParseBuffer(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        var parseStatus = ParserState.NeedMoreData;

        if (bytesConsumed >= bytesReady) return parseStatus; // We already can tell we need more data

        try
        {
            parseStatus = ParseStatusLine(buffer, bytesReady, ref bytesConsumed, ref _statusLineState, _maximumHeaderLength, ref _totalBytesConsumed, _currentToken, _httpResponse);
        }
        catch (Exception)
        {
            parseStatus = ParserState.Invalid;
        }

        return parseStatus;
    }

    private static ParserState ParseStatusLine(byte[] buffer, int bytesReady, ref int bytesConsumed, ref HttpStatusLineState statusLineState, int maximumHeaderLength, ref int totalBytesConsumed, StringBuilder currentToken, HttpUnsortedResponse httpResponse)
    {
        Contract.Assert((bytesReady - bytesConsumed) >= 0, "ParseRequestLine()|(bytesReady - bytesConsumed) < 0");
        Contract.Assert(maximumHeaderLength <= 0 || totalBytesConsumed <= maximumHeaderLength, "ParseRequestLine()|Headers already read exceeds limit.");

        // Remember where we started.
        var initialBytesParsed = bytesConsumed;
        int segmentStart;

        // Set up parsing status with what will happen if we exceed the buffer.
        var parseStatus = ParserState.DataTooBig;
        int effectiveMax = maximumHeaderLength <= 0 ? int.MaxValue : (maximumHeaderLength - totalBytesConsumed + bytesConsumed);
        if (bytesReady < effectiveMax)
        {
            parseStatus = ParserState.NeedMoreData;
            effectiveMax = bytesReady;
        }

        Contract.Assert(bytesConsumed < effectiveMax, "We have already consumed more than the max header length.");

        switch (statusLineState)
        {
            case HttpStatusLineState.BeforeVersionNumbers:
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
                        var token = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(token);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    var token = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(token);
                }

                // Validate value
                var version = currentToken.ToString();
                if (string.CompareOrdinal(FormattingUtilities.HttpVersionToken, version) != 0) throw new FormatException("HttpInvalidVersion");

                currentToken.Clear();

                // Move past the '/'
                statusLineState = HttpStatusLineState.MajorVersionNumber;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case HttpStatusLineState.MajorVersionNumber;

            case HttpStatusLineState.MajorVersionNumber:
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
                    string major = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(major);
                }

                // Move past the "."
                currentToken.Append('.');
                statusLineState = HttpStatusLineState.MinorVersionNumber;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HttpStatusLineState.MinorVersionNumber;

            case HttpStatusLineState.MinorVersionNumber:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != ' ')
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
                httpResponse.Version = Version.Parse(currentToken.ToString());
                currentToken.Clear();

                // Move past the SP
                statusLineState = HttpStatusLineState.StatusCode;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HttpStatusLineState.StatusCode;

            case HttpStatusLineState.StatusCode:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != ' ')
                {
                    if (buffer[bytesConsumed] < '0' || buffer[bytesConsumed] > '9')
                    {
                        parseStatus = ParserState.Invalid;
                        goto quit;
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        string method = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentToken.Append(method);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    string method = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentToken.Append(method);
                }

                // Copy value out
                int statusCode = int.Parse(currentToken.ToString(), CultureInfo.InvariantCulture);
                if (statusCode < 100 || statusCode > 1000)
                {
                    throw new FormatException("HttpInvalidStatusCode");
                }

                httpResponse.StatusCode = (HttpStatusCode)statusCode;
                currentToken.Clear();

                // Move past the SP
                statusLineState = HttpStatusLineState.ReasonPhrase;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HttpStatusLineState.ReasonPhrase;

            case HttpStatusLineState.ReasonPhrase:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != '\r')
                {
                    if (buffer[bytesConsumed] < 0x20 || buffer[bytesConsumed] > 0x7a)
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

                // Copy value out
                httpResponse.ReasonPhrase = currentToken.ToString();
                currentToken.Clear();

                // Move past the CR
                statusLineState = HttpStatusLineState.AfterCarriageReturn;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HttpStatusLineState.AfterCarriageReturn;

            case HttpStatusLineState.AfterCarriageReturn:
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