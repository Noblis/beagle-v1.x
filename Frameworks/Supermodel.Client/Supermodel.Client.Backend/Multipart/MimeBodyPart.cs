#nullable disable

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace Supermodel.Client.Backend.Multipart;

public class MimeBodyPart : IDisposable
{
    //private static readonly Type _streamType = typeof(Stream);
    private Stream _outputStream;
    private MultipartStreamProvider _streamProvider;
    private HttpContent _parentContent;
    private HttpContent _content;
    private HttpContentHeaders _headers;

    public MimeBodyPart(MultipartStreamProvider streamProvider, int maxBodyPartHeaderSize, HttpContent parentContent)
    {
        Contract.Assert(streamProvider != null);
        Contract.Assert(parentContent != null);
        _streamProvider = streamProvider;
        _parentContent = parentContent;
        Segments = new List<ArraySegment<byte>>(2);
        _headers = FormattingUtilities.CreateEmptyContentHeaders();
        HeaderParser = new InternetMessageFormatHeaderParser(_headers, maxBodyPartHeaderSize);
    }

    public InternetMessageFormatHeaderParser HeaderParser { get; private set; }

    public HttpContent GetCompletedHttpContent()
    {
        Contract.Assert(IsComplete);
        if (_content == null) return null;
        _headers.CopyTo(_content.Headers);
        return _content;
    }

    public List<ArraySegment<byte>> Segments { get; private set; }

    public bool IsComplete { get; set; }

    public bool IsFinal { get; set; }

    public async Task WriteSegment(ArraySegment<byte> segment, CancellationToken cancellationToken)
    {
        var stream = GetOutputStream();
        await stream.WriteAsync(segment.Array!, segment.Offset, segment.Count, cancellationToken);
    }

    private Stream GetOutputStream()
    {
        if (_outputStream == null)
        {
            try
            {
                _outputStream = _streamProvider.GetStream(_parentContent, _headers);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("ReadAsMimeMultipartStreamProviderException");
            }

            if (_outputStream == null)
            {
                throw new InvalidOperationException("ReadAsMimeMultipartStreamProviderNull");
            }

            if (!_outputStream.CanWrite)
            {
                throw new InvalidOperationException("ReadAsMimeMultipartStreamProviderReadOnly");
            }
            _content = new StreamContent(_outputStream);
        }

        return _outputStream;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            CleanupOutputStream();
            CleanupHttpContent();
            _parentContent = null;
            HeaderParser = null;
            Segments.Clear();
        }
    }

    private void CleanupHttpContent()
    {
        if (!IsComplete && _content != null) _content.Dispose();
        _content = null;
    }

    private void CleanupOutputStream()
    {
        if (_outputStream != null)
        {
            var output = _outputStream as MemoryStream;
            if (output != null)
            {
                output.Position = 0;
            }
            else
            {
                //_outputStream.Close(); 
                _outputStream.Dispose(); //Changed to accomodate WinRT
            }

            _outputStream = null;
        }
    }
}