#nullable disable

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;
using System.Text;

namespace Supermodel.Client.Backend.Multipart;

public enum ParserState
{
    NeedMoreData,
    Done,
    Invalid,
    DataTooBig,
}
    
public class InternetMessageFormatHeaderParser
{
    internal const int MinHeaderSize = 2;

    private int _totalBytesConsumed;
    private readonly int _maxHeaderSize;

    private HeaderFieldState _headerState;
    private readonly HttpHeaders _headers;
    private readonly CurrentHeaderFieldStore _currentHeader;

    public InternetMessageFormatHeaderParser(HttpHeaders headers, int maxHeaderSize)
    {
        // The minimum length which would be an empty header terminated by CRLF
        if (maxHeaderSize < MinHeaderSize) throw new ArgumentOutOfRangeException(nameof(maxHeaderSize));
        if (headers == null) throw new ArgumentNullException(nameof(headers));
        _headers = headers;
        _maxHeaderSize = maxHeaderSize;
        _currentHeader = new CurrentHeaderFieldStore();
    }

    private enum HeaderFieldState
    {
        Name = 0,
        Value,
        AfterCarriageReturn,
        FoldingLine
    }

    public ParserState ParseBuffer(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        var parseStatus = ParserState.NeedMoreData;

        if (bytesConsumed >= bytesReady) return parseStatus;  // We already can tell we need more data

        try
        {
            parseStatus = ParseHeaderFields(
                buffer,
                bytesReady,
                ref bytesConsumed,
                ref _headerState,
                _maxHeaderSize,
                ref _totalBytesConsumed,
                _currentHeader,
                _headers);
        }
        catch (Exception)
        {
            parseStatus = ParserState.Invalid;
        }

        return parseStatus;
    }

    private static ParserState ParseHeaderFields(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed,
        ref HeaderFieldState requestHeaderState,
        int maximumHeaderLength,
        ref int totalBytesConsumed,
        CurrentHeaderFieldStore currentField,
        HttpHeaders headers)
    {
        Contract.Assert((bytesReady - bytesConsumed) >= 0, "ParseHeaderFields()|(inputBufferLength - bytesParsed) < 0");
        Contract.Assert(maximumHeaderLength <= 0 || totalBytesConsumed <= maximumHeaderLength, "ParseHeaderFields()|Headers already read exceeds limit.");

        // Remember where we started.
        var initialBytesParsed = bytesConsumed;
        int segmentStart;

        // Set up parsing status with what will happen if we exceed the buffer.
        ParserState parseStatus = ParserState.DataTooBig;
        var effectiveMax = maximumHeaderLength <= 0 ? int.MaxValue : maximumHeaderLength - totalBytesConsumed + initialBytesParsed;
        if (bytesReady < effectiveMax)
        {
            parseStatus = ParserState.NeedMoreData;
            effectiveMax = bytesReady;
        }

        Contract.Assert(bytesConsumed < effectiveMax, "We have already consumed more than the max header length.");

        switch (requestHeaderState)
        {
            case HeaderFieldState.Name:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != ':')
                {
                    if (buffer[bytesConsumed] == '\r')
                    {
                        if (!currentField.IsEmpty())
                        {
                            parseStatus = ParserState.Invalid;
                            goto quit;
                        }
                        else
                        {
                            // Move past the '\r'
                            requestHeaderState = HeaderFieldState.AfterCarriageReturn;
                            if (++bytesConsumed == effectiveMax)
                            {
                                goto quit;
                            }

                            goto case HeaderFieldState.AfterCarriageReturn;
                        }
                    }

                    if (++bytesConsumed == effectiveMax)
                    {
                        string headerFieldName = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentField.Name.Append(headerFieldName);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    string headerFieldName = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentField.Name.Append(headerFieldName);
                }

                // Move past the ':'
                requestHeaderState = HeaderFieldState.Value;
                if (++bytesConsumed == effectiveMax)
                {
                    goto quit;
                }

                goto case HeaderFieldState.Value;

            case HeaderFieldState.Value:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != '\r')
                {
                    if (++bytesConsumed == effectiveMax)
                    {
                        string headerFieldValue = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                        currentField.Value.Append(headerFieldValue);
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    string headerFieldValue = Encoding.UTF8.GetString(buffer, segmentStart, bytesConsumed - segmentStart);
                    currentField.Value.Append(headerFieldValue);
                }

                // Move past the CR
                requestHeaderState = HeaderFieldState.AfterCarriageReturn;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case HeaderFieldState.AfterCarriageReturn;

            case HeaderFieldState.AfterCarriageReturn:
                if (buffer[bytesConsumed] != '\n')
                {
                    parseStatus = ParserState.Invalid;
                    goto quit;
                }

                if (currentField.IsEmpty())
                {
                    parseStatus = ParserState.Done;
                    bytesConsumed++;
                    goto quit;
                }

                requestHeaderState = HeaderFieldState.FoldingLine;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case HeaderFieldState.FoldingLine;

            case HeaderFieldState.FoldingLine:
                if (buffer[bytesConsumed] != ' ' && buffer[bytesConsumed] != '\t')
                {
                    currentField.CopyTo(headers);
                    requestHeaderState = HeaderFieldState.Name;
                    if (bytesConsumed == effectiveMax)
                    {
                        goto quit;
                    }

                    goto case HeaderFieldState.Name;
                }

                // Unfold line by inserting SP instead
                currentField.Value.Append(' ');

                // Continue parsing header field value
                requestHeaderState = HeaderFieldState.Value;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case HeaderFieldState.Value;
        }

        quit:
        totalBytesConsumed += bytesConsumed - initialBytesParsed;
        return parseStatus;
    }

    private class CurrentHeaderFieldStore
    {
        private const int DefaultFieldNameAllocation = 128;
        private const int DefaultFieldValueAllocation = 2 * 1024;

        private static readonly char[] _linearWhiteSpace = { ' ', '\t' };

        private readonly StringBuilder _name = new(DefaultFieldNameAllocation);
        private readonly StringBuilder _value = new(DefaultFieldValueAllocation);

        public StringBuilder Name
        {
            get { return _name; }
        }

        public StringBuilder Value
        {
            get { return _value; }
        }

        public void CopyTo(HttpHeaders headers)
        {
            headers.Add(_name.ToString(), _value.ToString().Trim(_linearWhiteSpace));
            Clear();
        }

        public bool IsEmpty()
        {
            return _name.Length == 0 && _value.Length == 0;
        }

        private void Clear()
        {
            _name.Clear();
            _value.Clear();
        }
    }
}