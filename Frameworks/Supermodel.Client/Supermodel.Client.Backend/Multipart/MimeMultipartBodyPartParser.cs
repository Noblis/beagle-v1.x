#nullable disable

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace Supermodel.Client.Backend.Multipart;

public class MimeMultipartBodyPartParser : IDisposable
{
    public const long DefaultMaxMessageSize = long.MaxValue;
    private const int DefaultMaxBodyPartHeaderSize = 4 * 1024;

    // MIME parser
    private MimeMultipartParser _mimeParser;
    private MimeMultipartParser.State _mimeStatus = MimeMultipartParser.State.NeedMoreData;
    private readonly ArraySegment<byte>[] _parsedBodyPart = new ArraySegment<byte>[2];
    private MimeBodyPart _currentBodyPart;
    private bool _isFirst = true;

    // Header field parser
    private ParserState _bodyPartHeaderStatus = ParserState.NeedMoreData;
    private readonly int _maxBodyPartHeaderSize;

    // Stream provider
    private readonly MultipartStreamProvider _streamProvider;

    private readonly HttpContent _content;

    public MimeMultipartBodyPartParser(HttpContent content, MultipartStreamProvider streamProvider) : this(content, streamProvider, DefaultMaxMessageSize, DefaultMaxBodyPartHeaderSize) {}

    public MimeMultipartBodyPartParser(
        HttpContent content,
        MultipartStreamProvider streamProvider,
        long maxMessageSize,
        int maxBodyPartHeaderSize)
    {
        Contract.Assert(content != null, "content cannot be null.");
        Contract.Assert(streamProvider != null, "streamProvider cannot be null.");

        string boundary = ValidateArguments(content, maxMessageSize, true);

        _mimeParser = new MimeMultipartParser(boundary, maxMessageSize);
        _currentBodyPart = new MimeBodyPart(streamProvider, maxBodyPartHeaderSize, content);
        _content = content;
        _maxBodyPartHeaderSize = maxBodyPartHeaderSize;

        _streamProvider = streamProvider;
    }

    public static bool IsMimeMultipartContent(HttpContent content)
    {
        Contract.Assert(content != null, "content cannot be null.");
        try
        {
            string boundary = ValidateArguments(content, DefaultMaxMessageSize, false);
            return boundary != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerable<MimeBodyPart> ParseBuffer(byte[] data, int bytesRead)
    {
        var bytesConsumed = 0;
        // There's a special case here - if we've reached the end of the message and there's no optional
        // CRLF, then we're out of bytes to read, but we have finished the message. 
        //
        // If IsWaitingForEndOfMessage is true and we're at the end of the stream, then we're going to 
        // call into the parser again with an empty array as the buffer to signal the end of the parse. 
        // Then the final boundary segment will be marked as complete. 
        if (bytesRead == 0 && !_mimeParser.IsWaitingForEndOfMessage)
        {
            CleanupCurrentBodyPart();
            throw new IOException("ReadAsMimeMultipartUnexpectedTermination");
        }

        // Make sure we remove an old array segments.
        _currentBodyPart.Segments.Clear();

        while (_mimeParser.CanParseMore(bytesRead, bytesConsumed))
        {
            bool isFinal;
            _mimeStatus = _mimeParser.ParseBuffer(data, bytesRead, ref bytesConsumed, out _parsedBodyPart[0], out _parsedBodyPart[1], out isFinal);
            if (_mimeStatus != MimeMultipartParser.State.BodyPartCompleted && _mimeStatus != MimeMultipartParser.State.NeedMoreData)
            {
                CleanupCurrentBodyPart();
                throw new InvalidOperationException("ReadAsMimeMultipartParseError");
            }

            // First body is empty preamble which we just ignore
            if (_isFirst)
            {
                if (_mimeStatus == MimeMultipartParser.State.BodyPartCompleted) _isFirst = false;
                continue;
            }

            // Parse the two array segments containing parsed body parts that the MIME parser gave us
            foreach (ArraySegment<byte> part in _parsedBodyPart)
            {
                if (part.Count == 0) continue;
                if (_bodyPartHeaderStatus != ParserState.Done)
                {
                    int headerConsumed = part.Offset;
                    _bodyPartHeaderStatus = _currentBodyPart.HeaderParser.ParseBuffer(part.Array, part.Count + part.Offset, ref headerConsumed);
                    if (_bodyPartHeaderStatus == ParserState.Done)
                    {
                        // Add the remainder as body part content
                        _currentBodyPart.Segments.Add(new ArraySegment<byte>(part.Array!, headerConsumed, part.Count + part.Offset - headerConsumed));
                    }
                    else if (_bodyPartHeaderStatus != ParserState.NeedMoreData)
                    {
                        CleanupCurrentBodyPart();
                        throw new InvalidOperationException("ReadAsMimeMultipartHeaderParseError");
                    }
                }
                else
                {
                    // Add the data as body part content
                    _currentBodyPart.Segments.Add(part);
                }
            }

            if (_mimeStatus == MimeMultipartParser.State.BodyPartCompleted)
            {
                // If body is completed then swap current body part
                MimeBodyPart completed = _currentBodyPart;
                completed.IsComplete = true;
                completed.IsFinal = isFinal;

                _currentBodyPart = new MimeBodyPart(_streamProvider, _maxBodyPartHeaderSize, _content);

                _mimeStatus = MimeMultipartParser.State.NeedMoreData;
                _bodyPartHeaderStatus = ParserState.NeedMoreData;
                yield return completed;
            }
            else
            {
                // Otherwise return what we have 
                yield return _currentBodyPart;
            }
        }
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mimeParser = null;
            CleanupCurrentBodyPart();
        }
    }

    private static string ValidateArguments(HttpContent content, long maxMessageSize, bool throwOnError)
    {
        Contract.Assert(content != null, "content cannot be null.");
        if (maxMessageSize < MimeMultipartParser.MinMessageSize)
        {
            if (throwOnError) throw new ArgumentOutOfRangeException(nameof(maxMessageSize));
            else return null;
        }

        MediaTypeHeaderValue contentType = content.Headers.ContentType;
        if (contentType == null)
        {
            if (throwOnError) throw new ArgumentException("ReadAsMimeMultipartArgumentNoContentType", nameof(content));
            else return null;
        }

        if (!contentType.MediaType!.StartsWith("multipart", StringComparison.OrdinalIgnoreCase))
        {
            if (throwOnError) throw new ArgumentException("ReadAsMimeMultipartArgumentNoMultipart", nameof(content));
            else return null;
        }

        string boundary = null;
        foreach (NameValueHeaderValue p in contentType.Parameters)
        {
            if (p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase))
            {
                boundary = FormattingUtilities.UnquoteToken(p.Value);
                break;
            }
        }

        if (boundary == null)
        {
            if (throwOnError) throw new ArgumentException("ReadAsMimeMultipartArgumentNoBoundary", nameof(content)); 
            else return null;
        }

        return boundary;
    }

    private void CleanupCurrentBodyPart()
    {
        if (_currentBodyPart != null)
        {
            _currentBodyPart.Dispose();
            _currentBodyPart = null;
        }
    }
}