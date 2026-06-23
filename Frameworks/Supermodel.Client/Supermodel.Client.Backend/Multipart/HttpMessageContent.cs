#nullable disable

using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Supermodel.Client.Backend.Multipart;

public class HttpMessageContent : HttpContent
{
    // ReSharper disable InconsistentNaming
    private const string SP = " ";
    private const string ColonSP = ": ";
    private const string CRLF = "\r\n";
    // ReSharper restore InconsistentNaming
    private const string CommaSeparator = ", ";

    private const int DefaultHeaderAllocation = 2 * 1024;

    private const string DefaultMediaType = "application/http";

    private const string MsgTypeParameter = "msgtype";
    private const string DefaultRequestMsgType = "request";
    private const string DefaultResponseMsgType = "response";

    //private const string DefaultRequestMediaType = DefaultMediaType + "; " + MsgTypeParameter + "=" + DefaultRequestMsgType;
    //private const string DefaultResponseMediaType = DefaultMediaType + "; " + MsgTypeParameter + "=" + DefaultResponseMsgType;

    // Set of header fields that only support single values such as Set-Cookie.
    private static readonly HashSet<string> _singleValueHeaderFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cookie",
        "Set-Cookie",
        "X-Powered-By",
    };

    // Set of header fields that should get serialized as space-separated values such as User-Agent.
    private static readonly HashSet<string> _spaceSeparatedValueHeaderFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "User-Agent",
    };

    private bool _contentConsumed;
    private Lazy<Task<Stream>> _streamTask;

    public HttpMessageContent(HttpRequestMessage httpRequest)
    {
        if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

        HttpRequestMessage = httpRequest;
        Headers.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
        Headers.ContentType.Parameters.Add(new NameValueHeaderValue(MsgTypeParameter, DefaultRequestMsgType));

        InitializeStreamTask();
    }

    public HttpMessageContent(HttpResponseMessage httpResponse)
    {
        if (httpResponse == null) throw new ArgumentNullException(nameof(httpResponse));

        HttpResponseMessage = httpResponse;
        Headers.ContentType = new MediaTypeHeaderValue(DefaultMediaType);
        Headers.ContentType.Parameters.Add(new NameValueHeaderValue(MsgTypeParameter, DefaultResponseMsgType));

        InitializeStreamTask();
    }

    private HttpContent Content
    {
        get { return HttpRequestMessage != null ? HttpRequestMessage.Content : HttpResponseMessage.Content; }
    }

    public HttpRequestMessage HttpRequestMessage { get; private set; }

    public HttpResponseMessage HttpResponseMessage { get; private set; }

    private void InitializeStreamTask()
    {
        _streamTask = new Lazy<Task<Stream>>(() => Content == null ? null : Content.ReadAsStreamAsync());
    }

    public static bool ValidateHttpMessageContent(HttpContent content, bool isRequest, bool throwOnError)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        var contentType = content.Headers.ContentType;
        if (contentType != null)
        {
            if (!contentType.MediaType!.Equals(DefaultMediaType, StringComparison.OrdinalIgnoreCase))
            {
                if (throwOnError) throw new ArgumentException("content");
                return false;
            }

            foreach (NameValueHeaderValue parameter in contentType.Parameters)
            {
                if (parameter.Name.Equals(MsgTypeParameter, StringComparison.OrdinalIgnoreCase))
                {
                    var msgType = FormattingUtilities.UnquoteToken(parameter.Value);
                    if (!msgType.Equals(isRequest ? DefaultRequestMsgType : DefaultResponseMsgType, StringComparison.OrdinalIgnoreCase))
                    {
                        if (throwOnError) throw new ArgumentException("content");
                        return false;
                    }

                    return true;
                }
            }
        }

        if (throwOnError) throw new ArgumentException("content");
        return false;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var header = SerializeHeader();
        await stream.WriteAsync(header, 0, header.Length);

        if (Content != null)
        {
            var readStream = await _streamTask.Value;
            ValidateStreamForReading(readStream);
            await Content.CopyToAsync(stream);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        // We have four states we could be in:
        //   1. We have content, but the task is still running or finished without success
        //   2. We have content, the task has finished successfully, and the stream came back as a null or non-seekable
        //   3. We have content, the task has finished successfully, and the stream is seekable, so we know its length
        //   4. We don't have content (streamTask.Value == null)
        //
        // For #1 and #2, we return false.
        // For #3, we return true & the size of our headers + the content length
        // For #4, we return true & the size of our headers

        var hasContent = _streamTask.Value != null;
        length = 0;

        // Cases #1, #2, #3
        if (hasContent)
        {
            Stream readStream;
            if (!_streamTask.Value.TryGetResult(out readStream) /* Case #1 */ || readStream == null || !readStream.CanSeek /* Case #2 */) 
            {
                length = -1;
                return false;
            }
            length = readStream.Length; // Case #3
        }

        // We serialize header to a StringBuilder so that we can determine the length
        // following the pattern for HttpContent to try and determine the message length.
        // The perf overhead is no larger than for the other HttpContent implementations.
        var header = SerializeHeader();
        length += header.Length;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (HttpRequestMessage != null)
            {
                HttpRequestMessage.Dispose();
                HttpRequestMessage = null;
            }

            if (HttpResponseMessage != null)
            {
                HttpResponseMessage.Dispose();
                HttpResponseMessage = null;
            }
        }

        base.Dispose(disposing);
    }

    private static void SerializeRequestLine(StringBuilder message, HttpRequestMessage httpRequest)
    {
        Contract.Assert(message != null, "message cannot be null");
        message.Append(httpRequest.Method + SP);
        message.Append(httpRequest.RequestUri!.PathAndQuery + SP);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        message.Append(FormattingUtilities.HttpVersionToken + "/" + (httpRequest.Version != null ? httpRequest.Version.ToString(2) : "1.1") + CRLF);

        // Only insert host header if not already present.
        if (httpRequest.Headers.Host == null)
        {
            message.Append(FormattingUtilities.HttpHostHeader + ColonSP + httpRequest.RequestUri.Authority + CRLF);
        }
    }

    private static void SerializeStatusLine(StringBuilder message, HttpResponseMessage httpResponse)
    {
        Contract.Assert(message != null, "message cannot be null");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        message.Append(FormattingUtilities.HttpVersionToken + "/" + (httpResponse.Version != null ? httpResponse.Version.ToString(2) : "1.1") + SP);
        message.Append((int)httpResponse.StatusCode + SP);
        message.Append(httpResponse.ReasonPhrase + CRLF);
    }

    private static void SerializeHeaderFields(StringBuilder message, HttpHeaders headers)
    {
        Contract.Assert(message != null, "message cannot be null");
        if (headers != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                if (_singleValueHeaderFields.Contains(header.Key))
                {
                    foreach (string value in header.Value) message.Append(header.Key + ColonSP + value + CRLF);
                }
                else if (_spaceSeparatedValueHeaderFields.Contains(header.Key))
                {
                    message.Append(header.Key + ColonSP + string.Join(SP, header.Value) + CRLF);
                }
                else
                {
                    message.Append(header.Key + ColonSP + string.Join(CommaSeparator, header.Value) + CRLF);
                }
            }
        }
    }

    private byte[] SerializeHeader()
    {
        var message = new StringBuilder(DefaultHeaderAllocation);
        HttpHeaders headers;
        HttpContent content;
        if (HttpRequestMessage != null)
        {
            SerializeRequestLine(message, HttpRequestMessage);
            headers = HttpRequestMessage.Headers;
            content = HttpRequestMessage.Content;
        }
        else
        {
            SerializeStatusLine(message, HttpResponseMessage);
            headers = HttpResponseMessage.Headers;
            content = HttpResponseMessage.Content;
        }

        SerializeHeaderFields(message, headers);
        if (content != null)
        {
            SerializeHeaderFields(message, content.Headers);
        }

        message.Append(CRLF);
        return Encoding.UTF8.GetBytes(message.ToString());
    }

    private void ValidateStreamForReading(Stream stream)
    {
        // If the content needs to be written to a target stream a 2nd time, then the stream must support
        // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
        // stream (e.g. a NetworkStream).
        if (_contentConsumed)
        {
            if (stream != null && stream.CanRead) stream.Position = 0;
            else throw new InvalidOperationException();
        }

        _contentConsumed = true;
    }
}