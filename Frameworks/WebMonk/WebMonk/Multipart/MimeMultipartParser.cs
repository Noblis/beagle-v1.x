#nullable disable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace WebMonk.Multipart;

public class MimeMultipartParser
{
    public const int MinMessageSize = 10;

    private const int MaxBoundarySize = 256;

    // ReSharper disable InconsistentNaming
    private const byte HTAB = 0x09;
    private const byte SP = 0x20;
    private const byte CR = 0x0D;
    private const byte LF = 0x0A;
    // ReSharper restore InconsistentNaming
    private const byte Dash = 0x2D;
    private static readonly ArraySegment<byte> _emptyBodyPart = new([]);

    private long _totalBytesConsumed;
    private readonly long _maxMessageSize;

    private BodyPartState _bodyPartState;
    private readonly CurrentBodyPartStore _currentBoundary;

    public MimeMultipartParser(string boundary, long maxMessageSize)
    {
        // The minimum length which would be an empty message terminated by CRLF
        if (maxMessageSize < MinMessageSize) throw new ArgumentOutOfRangeException(nameof(maxMessageSize));
        if (string.IsNullOrWhiteSpace(boundary)) throw new ArgumentNullException(nameof(boundary));
        if (boundary.Length > MaxBoundarySize - 10) throw new ArgumentOutOfRangeException(nameof(boundary));
        if (boundary.EndsWith(" ", StringComparison.Ordinal)) throw new ArgumentException("MimeMultipartParserBadBoundary", nameof(boundary));

        _maxMessageSize = maxMessageSize;
        _currentBoundary = new CurrentBodyPartStore(boundary);
        _bodyPartState = BodyPartState.AfterFirstLineFeed;
    }

    public bool IsWaitingForEndOfMessage
    {
        get
        {
            return
                _bodyPartState == BodyPartState.AfterBoundary &&
                _currentBoundary != null &&
                _currentBoundary.IsFinal;
        }
    }

    private enum BodyPartState
    {
        BodyPart = 0,
        AfterFirstCarriageReturn,
        AfterFirstLineFeed,
        AfterFirstDash,
        Boundary,
        AfterBoundary,
        AfterSecondDash,
        AfterSecondCarriageReturn
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedType.Local
    private enum MessageState
    {
        Boundary = 0, // about to parse boundary
        // ReSharper disable once UnusedMember.Local
        BodyPart, // about to parse body-part
        // ReSharper disable once UnusedMember.Local
        CloseDelimiter // about to read close-delimiter
    }

    public enum State
    {
        /// <summary>
        /// Need more data
        /// </summary>
        NeedMoreData = 0,

        /// <summary>
        /// Parsing of a complete body part succeeded.
        /// </summary>
        BodyPartCompleted,

        /// <summary>
        /// Bad data format
        /// </summary>
        Invalid,

        /// <summary>
        /// Data exceeds the allowed size
        /// </summary>
        DataTooBig,
    }

    public bool CanParseMore(int bytesRead, int bytesConsumed)
    {
        if (bytesConsumed < bytesRead)
        {
            // If there's more bytes we haven't parsed, then we can parse more
            return true;
        }

        if (bytesRead == 0 && IsWaitingForEndOfMessage)
        {
            // If we're waiting for the end of the message and we've arrived there, we want parse to be called
            // again so we can mark the parse as complete.
            //
            // This can happen when the last boundary segment doesn't have a trailing CRLF. We need to wait until
            // the end of the message to complete the parse because we need to consume any trailing whitespace that's
            //present.
            return true;
        }

        return false;
    }

    public State ParseBuffer(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed,
        out ArraySegment<byte> remainingBodyPart,
        out ArraySegment<byte> bodyPart,
        out bool isFinalBodyPart)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));

        State parseStatus;
        isFinalBodyPart = false;

        try
        {
            parseStatus = ParseBodyPart(
                buffer,
                bytesReady,
                ref bytesConsumed,
                ref _bodyPartState,
                _maxMessageSize,
                ref _totalBytesConsumed,
                _currentBoundary);
        }
        catch (Exception)
        {
            parseStatus = State.Invalid;
        }

        remainingBodyPart = _currentBoundary.GetDiscardedBoundary();
        bodyPart = _currentBoundary.BodyPart;
        if (parseStatus == State.BodyPartCompleted)
        {
            isFinalBodyPart = _currentBoundary.IsFinal;
            _currentBoundary.ClearAll();
        }
        else
        {
            _currentBoundary.ClearBodyPart();
        }

        return parseStatus;
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This is a parser which cannot be split up for performance reasons.")]
    private static State ParseBodyPart(
        byte[] buffer,
        int bytesReady,
        ref int bytesConsumed,
        ref BodyPartState bodyPartState,
        long maximumMessageLength,
        ref long totalBytesConsumed,
        CurrentBodyPartStore currentBodyPart)
    {
        Contract.Assert((bytesReady - bytesConsumed) >= 0, "ParseBodyPart()|(bytesReady - bytesConsumed) < 0");
        Contract.Assert(maximumMessageLength <= 0 || totalBytesConsumed <= maximumMessageLength, "ParseBodyPart()|Message already read exceeds limit.");

        // Remember where we started.
        int segmentStart;
        var initialBytesParsed = bytesConsumed;

        if (bytesReady == 0 && bodyPartState == BodyPartState.AfterBoundary && currentBodyPart.IsFinal)
        {
            // We've seen the end of the stream - the final body part has no trailing CRLF
            return State.BodyPartCompleted;
        }

        // Set up parsing status with what will happen if we exceed the buffer.
        State parseStatus = State.DataTooBig;
        var effectiveMax = maximumMessageLength <= 0 ? long.MaxValue : (maximumMessageLength - totalBytesConsumed + bytesConsumed);
        if (effectiveMax == 0)
        {
            // effectiveMax is based on our max message size - if we've arrrived at the max size, then we need
            // to stop parsing.
            return State.DataTooBig;
        }

        if (bytesReady <= effectiveMax)
        {
            parseStatus = State.NeedMoreData;
            effectiveMax = bytesReady;
        }

        currentBodyPart.ResetBoundaryOffset();

        Contract.Assert(bytesConsumed < effectiveMax, "We have already consumed more than the max header length.");

        switch (bodyPartState)
        {
            case BodyPartState.BodyPart:
                while (buffer[bytesConsumed] != CR)
                {
                    if (++bytesConsumed == effectiveMax) goto quit;
                }

                // Remember potential boundary
                currentBodyPart.AppendBoundary(CR);

                // Move past the CR
                bodyPartState = BodyPartState.AfterFirstCarriageReturn;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case BodyPartState.AfterFirstCarriageReturn;

            case BodyPartState.AfterFirstCarriageReturn:
                if (buffer[bytesConsumed] != LF)
                {
                    currentBodyPart.ResetBoundary();
                    bodyPartState = BodyPartState.BodyPart;
                    goto case BodyPartState.BodyPart;
                }

                // Remember potential boundary
                currentBodyPart.AppendBoundary(LF);

                // Move past the CR
                bodyPartState = BodyPartState.AfterFirstLineFeed;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case BodyPartState.AfterFirstLineFeed;

            case BodyPartState.AfterFirstLineFeed:
                if (buffer[bytesConsumed] == CR)
                {
                    // Remember potential boundary
                    currentBodyPart.ResetBoundary();
                    currentBodyPart.AppendBoundary(CR);

                    // Move past the CR
                    bodyPartState = BodyPartState.AfterFirstCarriageReturn;
                    if (++bytesConsumed == effectiveMax) goto quit;

                    goto case BodyPartState.AfterFirstCarriageReturn;
                }

                if (buffer[bytesConsumed] != Dash)
                {
                    currentBodyPart.ResetBoundary();
                    bodyPartState = BodyPartState.BodyPart;
                    goto case BodyPartState.BodyPart;
                }

                // Remember potential boundary
                currentBodyPart.AppendBoundary(Dash);

                // Move past the Dash
                bodyPartState = BodyPartState.AfterFirstDash;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case BodyPartState.AfterFirstDash;

            case BodyPartState.AfterFirstDash:
                if (buffer[bytesConsumed] != Dash)
                {
                    currentBodyPart.ResetBoundary();
                    bodyPartState = BodyPartState.BodyPart;
                    goto case BodyPartState.BodyPart;
                }

                // Remember potential boundary
                currentBodyPart.AppendBoundary(Dash);

                // Move past the Dash
                bodyPartState = BodyPartState.Boundary;
                if (++bytesConsumed == effectiveMax) goto quit;

                goto case BodyPartState.Boundary;

            case BodyPartState.Boundary:
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != CR)
                {
                    if (++bytesConsumed == effectiveMax)
                    {
                        if (currentBodyPart.AppendBoundary(buffer, segmentStart, bytesConsumed - segmentStart))
                        {
                            if (currentBodyPart.IsBoundaryComplete())
                            {
                                // At this point we've seen the end of a boundary segment that is aligned at the end
                                // of the buffer - this might be because we have another segment coming or it might
                                // truly be the end of the message.
                                bodyPartState = BodyPartState.AfterBoundary;
                            }
                        }
                        else
                        {
                            currentBodyPart.ResetBoundary();
                            bodyPartState = BodyPartState.BodyPart;
                        }
                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    if (!currentBodyPart.AppendBoundary(buffer, segmentStart, bytesConsumed - segmentStart))
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = BodyPartState.BodyPart;
                        goto case BodyPartState.BodyPart;
                    }
                }

                goto case BodyPartState.AfterBoundary;

            case BodyPartState.AfterBoundary:

                // This state means that we just saw the end of a boundary. It might by a 'normal' boundary, in which
                // case it's followed by optional whitespace and a CRLF. Or it might be the 'final' boundary and will 
                // be followed by '--', optional whitespace and an optional CRLF.
                if (buffer[bytesConsumed] == Dash && !currentBodyPart.IsFinal)
                {
                    currentBodyPart.AppendBoundary(Dash);
                    if (++bytesConsumed == effectiveMax)
                    {
                        bodyPartState = BodyPartState.AfterSecondDash;
                        goto quit;
                    }

                    goto case BodyPartState.AfterSecondDash;
                }

                // Capture optional whitespace
                segmentStart = bytesConsumed;
                while (buffer[bytesConsumed] != CR)
                {
                    if (++bytesConsumed == effectiveMax)
                    {
                        if (!currentBodyPart.AppendBoundary(buffer, segmentStart, bytesConsumed - segmentStart))
                        {
                            // It's an unexpected character
                            currentBodyPart.ResetBoundary();
                            bodyPartState = BodyPartState.BodyPart;
                        }

                        goto quit;
                    }
                }

                if (bytesConsumed > segmentStart)
                {
                    if (!currentBodyPart.AppendBoundary(buffer, segmentStart, bytesConsumed - segmentStart))
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = BodyPartState.BodyPart;
                        goto case BodyPartState.BodyPart;
                    }
                }

                if (buffer[bytesConsumed] == CR)
                {
                    currentBodyPart.AppendBoundary(CR);
                    if (++bytesConsumed == effectiveMax)
                    {
                        bodyPartState = BodyPartState.AfterSecondCarriageReturn;
                        goto quit;
                    }

                    goto case BodyPartState.AfterSecondCarriageReturn;
                }
                else
                {
                    // It's an unexpected character
                    currentBodyPart.ResetBoundary();
                    bodyPartState = BodyPartState.BodyPart;
                    goto case BodyPartState.BodyPart;
                }

            case BodyPartState.AfterSecondDash:
                if (buffer[bytesConsumed] == Dash)
                {
                    currentBodyPart.AppendBoundary(Dash);
                    bytesConsumed++;

                    if (currentBodyPart.IsBoundaryComplete())
                    {
                        Debug.Assert(currentBodyPart.IsFinal);

                        // If we get in here, it means we've see the trailing '--' of the last boundary - in order to consume all of the 
                        // remaining bytes, we don't mark the parse as complete again - wait until this method is called again with the 
                        // empty buffer to do that.
                        bodyPartState = BodyPartState.AfterBoundary;
                        parseStatus = State.NeedMoreData;
                        goto quit;
                    }
                    else
                    {
                        currentBodyPart.ResetBoundary();
                        if (bytesConsumed == effectiveMax) goto quit;

                        goto case BodyPartState.BodyPart;
                    }
                }
                else
                {
                    currentBodyPart.ResetBoundary();
                    bodyPartState = BodyPartState.BodyPart;
                    goto case BodyPartState.BodyPart;
                }

            case BodyPartState.AfterSecondCarriageReturn:
                if (buffer[bytesConsumed] != LF)
                {
                    currentBodyPart.ResetBoundary();
                    bodyPartState = BodyPartState.BodyPart;
                    goto case BodyPartState.BodyPart;
                }

                currentBodyPart.AppendBoundary(LF);
                bytesConsumed++;

                bodyPartState = BodyPartState.BodyPart;
                if (currentBodyPart.IsBoundaryComplete())
                {
                    parseStatus = State.BodyPartCompleted;
                    goto quit;
                }
                else
                {
                    currentBodyPart.ResetBoundary();
                    if (bytesConsumed == effectiveMax) goto quit;

                    goto case BodyPartState.BodyPart;
                }
        }

        quit:
        if (initialBytesParsed < bytesConsumed)
        {
            int boundaryLength = currentBodyPart.BoundaryDelta;
            if (boundaryLength > 0 && parseStatus != State.BodyPartCompleted)
            {
                currentBodyPart.HasPotentialBoundaryLeftOver = true;
            }

            int bodyPartEnd = bytesConsumed - initialBytesParsed - boundaryLength;

            currentBodyPart.BodyPart = new ArraySegment<byte>(buffer, initialBytesParsed, bodyPartEnd);
        }

        totalBytesConsumed += bytesConsumed - initialBytesParsed;
        return parseStatus;
    }

    private class CurrentBodyPartStore
    {
        private const int InitialOffset = 2;

        private readonly byte[] _boundaryStore = new byte[MaxBoundarySize];
        private int _boundaryStoreLength;

        private readonly byte[] _referenceBoundary = new byte[MaxBoundarySize];
        private readonly int _referenceBoundaryLength;

        private readonly byte[] _boundary = new byte[MaxBoundarySize];
        private int _boundaryLength;

        private ArraySegment<byte> _bodyPart = _emptyBodyPart;
        private bool _isFinal;
        private bool _isFirst = true;
        private bool _releaseDiscardedBoundary;
        private int _boundaryOffset;

        public CurrentBodyPartStore(string referenceBoundary)
        {
            Contract.Assert(referenceBoundary != null);

            _referenceBoundary[0] = CR;
            _referenceBoundary[1] = LF;
            _referenceBoundary[2] = Dash;
            _referenceBoundary[3] = Dash;
            _referenceBoundaryLength = 4 + Encoding.UTF8.GetBytes(referenceBoundary, 0, referenceBoundary.Length, _referenceBoundary, 4);

            _boundary[0] = CR;
            _boundary[1] = LF;
            _boundaryLength = InitialOffset;
        }

        public bool HasPotentialBoundaryLeftOver { get; set; }

        public int BoundaryDelta
        {
            get { return (_boundaryLength - _boundaryOffset > 0) ? _boundaryLength - _boundaryOffset : _boundaryLength; }
        }

        public ArraySegment<byte> BodyPart
        {
            get { return _bodyPart; }
            set { _bodyPart = value; }
        }

        public bool IsFinal
        {
            get { return _isFinal; }
        }

        public void ResetBoundaryOffset()
        {
            _boundaryOffset = _boundaryLength;
        }

        public void ResetBoundary()
        {
            // If we had a potential boundary left over then store it so that we don't loose it
            if (HasPotentialBoundaryLeftOver)
            {
                Buffer.BlockCopy(_boundary, 0, _boundaryStore, 0, _boundaryOffset);
                _boundaryStoreLength = _boundaryOffset;
                HasPotentialBoundaryLeftOver = false;
                _releaseDiscardedBoundary = true;
            }

            _boundaryLength = 0;
            _boundaryOffset = 0;
        }

        public void AppendBoundary(byte data)
        {
            _boundary[_boundaryLength++] = data;
        }

        public bool AppendBoundary(byte[] data, int offset, int count)
        {
            // Check that potential boundary is not bigger than our reference boundary. 
            // Allow for 2 extra characters to include the final boundary which ends with 
            // an additional "--" sequence + plus up to 4 LWS characters (which are allowed). 
            if (_boundaryLength + count > _referenceBoundaryLength + 6) return false;

            var cnt = _boundaryLength;
            Buffer.BlockCopy(data, offset, _boundary, _boundaryLength, count);
            _boundaryLength += count;

            // Verify that boundary matches so far
            var maxCount = Math.Min(_boundaryLength, _referenceBoundaryLength);
            for (; cnt < maxCount; cnt++)
            {
                if (_boundary[cnt] != _referenceBoundary[cnt]) return false;
            }

            return true;
        }

        public ArraySegment<byte> GetDiscardedBoundary()
        {
            if (_boundaryStoreLength > 0 && _releaseDiscardedBoundary)
            {
                ArraySegment<byte> discarded = new ArraySegment<byte>(_boundaryStore, 0, _boundaryStoreLength);
                _boundaryStoreLength = 0;
                return discarded;
            }

            return _emptyBodyPart;
        }

        public bool IsBoundaryValid()
        {
            var offset = 0;
            if (_isFirst)
            {
                offset = InitialOffset;
            }

            var count = offset;
            for (; count < _referenceBoundaryLength; count++)
            {
                if (_boundary[count] != _referenceBoundary[count])
                {
                    return false;
                }
            }

            // Check for final
            var boundaryIsFinal = false;
            if (_boundary[count] == Dash &&
                _boundary[count + 1] == Dash)
            {
                boundaryIsFinal = true;
                count += 2;
            }

            // Rest of boundary must be ignorable whitespace in order for it to match
            for (; count < _boundaryLength - 2; count++)
            {
                if (_boundary[count] != SP && _boundary[count] != HTAB)
                {
                    return false;
                }
            }

            // We have a valid boundary so whatever we stored in the boundary story is no longer needed
            _isFinal = boundaryIsFinal;
            _isFirst = false;

            return true;
        }

        public bool IsBoundaryComplete()
        {
            if (!IsBoundaryValid()) return false;
            if (_boundaryLength < _referenceBoundaryLength) return false;
            if (_boundaryLength == _referenceBoundaryLength + 1 && _boundary[_referenceBoundaryLength] == Dash) return false;
            return true;
        }

        public void ClearBodyPart()
        {
            BodyPart = _emptyBodyPart;
        }

        public void ClearAll()
        {
            _releaseDiscardedBoundary = false;
            HasPotentialBoundaryLeftOver = false;
            _boundaryLength = 0;
            _boundaryOffset = 0;
            _boundaryStoreLength = 0;
            _isFinal = false;
            ClearBodyPart();
        }

        // ReSharper disable once UnusedMember.Local
        private string DebuggerToString()
        {
            var referenceBoundary = Encoding.UTF8.GetString(_referenceBoundary, 0, _referenceBoundaryLength);
            var boundary = Encoding.UTF8.GetString(_boundary, 0, _boundaryLength);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Expected: {0} *** Current: {1}",
                referenceBoundary,
                boundary);
        }
    }
}