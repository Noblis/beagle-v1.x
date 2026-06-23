#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WebMonk.Multipart;

public static class HttpContentMessageExtensions
{
    private const int MinBufferSize = 256;
    private const int DefaultBufferSize = 32 * 1024;

    public static bool IsHttpRequestMessageContent(this HttpContent content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        try
        {
            return HttpMessageContent.ValidateHttpMessageContent(content, true, false);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool IsHttpResponseMessageContent(this HttpContent content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        try
        {
            return HttpMessageContent.ValidateHttpMessageContent(content, false, false);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content)
    {
        return ReadAsHttpRequestMessageAsync(content, "http", DefaultBufferSize);
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        return ReadAsHttpRequestMessageAsync(content, "http", DefaultBufferSize, cancellationToken);
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme)
    {
        return ReadAsHttpRequestMessageAsync(content, uriScheme, DefaultBufferSize);
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, CancellationToken cancellationToken)
    {
        return ReadAsHttpRequestMessageAsync(content, uriScheme, DefaultBufferSize, cancellationToken);
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize, CancellationToken cancellationToken)
    {
        return ReadAsHttpRequestMessageAsync(content, uriScheme, bufferSize, HttpRequestHeaderParser.DefaultMaxHeaderSize, cancellationToken);
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize, int maxHeaderSize = HttpRequestHeaderParser.DefaultMaxHeaderSize)
    {
        return ReadAsHttpRequestMessageAsync(content, uriScheme, bufferSize, maxHeaderSize, CancellationToken.None);
    }

    public static Task<HttpRequestMessage> ReadAsHttpRequestMessageAsync(this HttpContent content, string uriScheme, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (uriScheme == null) throw new ArgumentNullException(nameof(uriScheme));
        if (!Uri.CheckSchemeName(uriScheme)) throw new ArgumentException("HttpMessageParserInvalidUriScheme", nameof(uriScheme));
        if (bufferSize < MinBufferSize) throw new ArgumentOutOfRangeException(nameof(bufferSize));
        if (maxHeaderSize < InternetMessageFormatHeaderParser.MinHeaderSize) throw new ArgumentOutOfRangeException(nameof(maxHeaderSize));

        HttpMessageContent.ValidateHttpMessageContent(content, true, true);

        return content.ReadAsHttpRequestMessageAsyncCore(uriScheme, bufferSize, maxHeaderSize, cancellationToken);
    }

    private static async Task<HttpRequestMessage> ReadAsHttpRequestMessageAsyncCore(this HttpContent content, string uriScheme, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = await content.ReadAsStreamAsync();

        var httpRequest = new HttpUnsortedRequest();
        var parser = new HttpRequestHeaderParser(httpRequest, HttpRequestHeaderParser.DefaultMaxRequestLineSize, maxHeaderSize);

        var buffer = new byte[bufferSize];
        var headerConsumed = 0;

        while (true)
        {
            int bytesRead;
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            }
            catch (Exception e)
            {
                throw new IOException("HttpMessageErrorReading", e);
            }

            ParserState parseStatus;
            try
            {
                parseStatus = parser.ParseBuffer(buffer, bytesRead, ref headerConsumed);
            }
            catch (Exception)
            {
                parseStatus = ParserState.Invalid;
            }

            if (parseStatus == ParserState.Done) return CreateHttpRequestMessage(uriScheme, httpRequest, stream, bytesRead - headerConsumed);
            if (parseStatus != ParserState.NeedMoreData) throw new InvalidOperationException("HttpMessageParserError");
            if (bytesRead == 0) throw new IOException("ReadAsHttpMessageUnexpectedTermination");
        }
    }

    public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content)
    {
        return ReadAsHttpResponseMessageAsync(content, DefaultBufferSize);
    }

    public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        return ReadAsHttpResponseMessageAsync(content, DefaultBufferSize, cancellationToken);
    }

    public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize)
    {
        return ReadAsHttpResponseMessageAsync(content, bufferSize, HttpResponseHeaderParser.DefaultMaxHeaderSize);
    }

    public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize,
        CancellationToken cancellationToken)
    {
        return ReadAsHttpResponseMessageAsync(content, bufferSize, HttpResponseHeaderParser.DefaultMaxHeaderSize, cancellationToken);
    }

    public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize, int maxHeaderSize)
    {
        return ReadAsHttpResponseMessageAsync(content, bufferSize, maxHeaderSize, CancellationToken.None);
    }

    public static Task<HttpResponseMessage> ReadAsHttpResponseMessageAsync(this HttpContent content, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (bufferSize < MinBufferSize) throw new ArgumentOutOfRangeException(nameof(bufferSize));
        if (maxHeaderSize < InternetMessageFormatHeaderParser.MinHeaderSize) throw new ArgumentOutOfRangeException(nameof(maxHeaderSize));
        HttpMessageContent.ValidateHttpMessageContent(content, false, true);

        return content.ReadAsHttpResponseMessageAsyncCore(bufferSize, maxHeaderSize, cancellationToken);
    }

    private static async Task<HttpResponseMessage> ReadAsHttpResponseMessageAsyncCore(this HttpContent content, int bufferSize, int maxHeaderSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = await content.ReadAsStreamAsync();

        var httpResponse = new HttpUnsortedResponse();
        var parser = new HttpResponseHeaderParser(httpResponse, HttpResponseHeaderParser.DefaultMaxStatusLineSize, maxHeaderSize);
        ParserState parseStatus;

        var buffer = new byte[bufferSize];
        int bytesRead;
        var headerConsumed = 0;

        while (true)
        {
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            }
            catch (Exception e)
            {
                throw new IOException("HttpMessageErrorReading", e);
            }

            try
            {
                parseStatus = parser.ParseBuffer(buffer, bytesRead, ref headerConsumed);
            }
            catch (Exception)
            {
                parseStatus = ParserState.Invalid;
            }

            if (parseStatus == ParserState.Done) return CreateHttpResponseMessage(httpResponse, stream, bytesRead - headerConsumed); // Create and return parsed HttpResponseMessage
            else if (parseStatus != ParserState.NeedMoreData) throw new InvalidOperationException("HttpMessageParserError");
            else if (bytesRead == 0) throw new IOException("ReadAsHttpMessageUnexpectedTermination");
        }
    }

    private static Uri CreateRequestUri(string uriScheme, HttpUnsortedRequest httpRequest)
    {
        Contract.Assert(httpRequest != null, "httpRequest cannot be null.");
        Contract.Assert(uriScheme != null, "uriScheme cannot be null");

        IEnumerable<string> hostValues;
        if (httpRequest.HttpHeaders.TryGetValues(FormattingUtilities.HttpHostHeader, out hostValues))
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var hostCount = hostValues.Count();
            if (hostCount != 1) throw new InvalidOperationException("HttpMessageParserInvalidHostCount");
        }
        else
        {
            throw new InvalidOperationException("HttpMessageParserInvalidHostCount");
        }

        // We don't use UriBuilder as hostValues.ElementAt(0) contains 'host:port' and UriBuilder needs these split out into separate host and port.
        // ReSharper disable once PossibleMultipleEnumeration
        var requestUri = string.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}", uriScheme, hostValues.ElementAt(0), httpRequest.RequestUri);
        return new Uri(requestUri);
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    private static HttpContent CreateHeaderFields(HttpHeaders source, HttpHeaders destination, Stream contentStream, int rewind)
    {
        Contract.Assert(source != null, "source headers cannot be null");
        Contract.Assert(destination != null, "destination headers cannot be null");
        Contract.Assert(contentStream != null, "contentStream must be non null");
        HttpContentHeaders contentHeaders = null;
        HttpContent content = null;

        // Set the header fields
        foreach (KeyValuePair<string, IEnumerable<string>> header in source)
        {
            if (!destination.TryAddWithoutValidation(header.Key, header.Value))
            {
                if (contentHeaders == null) contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
                contentHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // If we have content headers then create an HttpContent for this Response
        if (contentHeaders != null)
        {
            // Need to rewind the input stream to be at the position right after the HTTP header
            // which we may already have parsed as we read the content stream.
            if (!contentStream.CanSeek) throw new InvalidOperationException("HttpMessageContentStreamMustBeSeekable");

            contentStream.Seek(0 - rewind, SeekOrigin.Current);
            content = new StreamContent(contentStream);
            contentHeaders.CopyTo(content.Headers);
        }

        return content;
    }

    private static HttpRequestMessage CreateHttpRequestMessage(string uriScheme, HttpUnsortedRequest httpRequest, Stream contentStream, int rewind)
    {
        Contract.Assert(uriScheme != null, "URI scheme must be non null");
        Contract.Assert(httpRequest != null, "httpRequest must be non null");
        Contract.Assert(contentStream != null, "contentStream must be non null");

        HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

        // Set method, requestURI, and version
        httpRequestMessage.Method = httpRequest.Method;
        httpRequestMessage.RequestUri = CreateRequestUri(uriScheme, httpRequest);
        httpRequestMessage.Version = httpRequest.Version;

        // Set the header fields and content if any
        httpRequestMessage.Content = CreateHeaderFields(httpRequest.HttpHeaders, httpRequestMessage.Headers, contentStream, rewind);

        return httpRequestMessage;
    }

    private static HttpResponseMessage CreateHttpResponseMessage(HttpUnsortedResponse httpResponse, Stream contentStream, int rewind)
    {
        Contract.Assert(httpResponse != null, "httpResponse must be non null");
        Contract.Assert(contentStream != null, "contentStream must be non null");

        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

        // Set version, status code and reason phrase
        httpResponseMessage.Version = httpResponse.Version;
        httpResponseMessage.StatusCode = httpResponse.StatusCode;
        httpResponseMessage.ReasonPhrase = httpResponse.ReasonPhrase;

        // Set the header fields and content if any
        httpResponseMessage.Content = CreateHeaderFields(httpResponse.HttpHeaders, httpResponseMessage.Headers, contentStream, rewind);

        return httpResponseMessage;
    }
}