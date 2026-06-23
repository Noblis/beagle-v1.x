#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebMonk.Multipart;

public static class HttpContentMultipartExtensions
{
    private const int MinBufferSize = 256;
    private const int DefaultBufferSize = 32 * 1024;

    public static bool IsMimeMultipartContent(this HttpContent content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        return MimeMultipartBodyPartParser.IsMimeMultipartContent(content);
    }

    public static bool IsMimeMultipartContent(this HttpContent content, string subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype)) throw new ArgumentNullException(nameof(subtype));
        if (IsMimeMultipartContent(content))
        {
            if (content.Headers.ContentType.MediaType.Equals("multipart/" + subtype, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    public static Task<MultipartMemoryStreamProvider> ReadAsMultipartAsync(this HttpContent content)
    {
        return ReadAsMultipartAsync(content, new MultipartMemoryStreamProvider(), DefaultBufferSize);
    }

    public static Task<MultipartMemoryStreamProvider> ReadAsMultipartAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        return ReadAsMultipartAsync(content, new MultipartMemoryStreamProvider(), DefaultBufferSize, cancellationToken);
    }

    public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider) where T : MultipartStreamProvider
    {
        return ReadAsMultipartAsync(content, streamProvider, DefaultBufferSize);
    }

    public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider, CancellationToken cancellationToken) where T : MultipartStreamProvider
    {
        return ReadAsMultipartAsync(content, streamProvider, DefaultBufferSize, cancellationToken);
    }

    public static Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider, int bufferSize) where T : MultipartStreamProvider
    {
        return ReadAsMultipartAsync(content, streamProvider, bufferSize, CancellationToken.None);
    }

    public static async Task<T> ReadAsMultipartAsync<T>(this HttpContent content, T streamProvider, int bufferSize, CancellationToken cancellationToken) where T : MultipartStreamProvider
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (streamProvider == null) throw new ArgumentNullException(nameof(streamProvider));

        if (bufferSize < MinBufferSize) throw new ArgumentOutOfRangeException(nameof(bufferSize));

        Stream stream;
        try
        {
            stream = await content.ReadAsStreamAsync();
        }
        catch (Exception e)
        {
            throw new IOException("ReadAsMimeMultipartErrorReading", e);
        }

        using (var parser = new MimeMultipartBodyPartParser(content, streamProvider))
        {
            byte[] data = new byte[bufferSize];
            MultipartAsyncContext context = new MultipartAsyncContext(stream, parser, data, streamProvider.Contents);

            // Start async read/write loop
            await MultipartReadAsync(context, cancellationToken);

            // Let the stream provider post-process when everything is complete
            await streamProvider.ExecutePostProcessingAsync(cancellationToken);
            return streamProvider;
        }
    }

    private static async Task MultipartReadAsync(MultipartAsyncContext context, CancellationToken cancellationToken)
    {
        Contract.Assert(context != null, "context cannot be null");
        while (true)
        {
            int bytesRead;
            try
            {
                bytesRead = await context.ContentStream.ReadAsync(context.Data, 0, context.Data.Length, cancellationToken);
            }
            catch (Exception e)
            {
                throw new IOException("ReadAsMimeMultipartErrorReading", e);
            }

            IEnumerable<MimeBodyPart> parts = context.MimeParser.ParseBuffer(context.Data, bytesRead);

            foreach (MimeBodyPart part in parts)
            {
                foreach (ArraySegment<byte> segment in part.Segments)
                {
                    try
                    {
                        await part.WriteSegment(segment, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        part.Dispose();
                        throw new IOException("ReadAsMimeMultipartErrorWriting", e);
                    }
                }

                if (CheckIsFinalPart(part, context.Result)) return;
            }
        }
    }

    private static bool CheckIsFinalPart(MimeBodyPart part, ICollection<HttpContent> result)
    {
        Contract.Assert(part != null, "part cannot be null.");
        Contract.Assert(result != null, "result cannot be null.");
        if (part.IsComplete)
        {
            var partContent = part.GetCompletedHttpContent();
            if (partContent != null)
            {
                result.Add(partContent);
            }

            bool isFinal = part.IsFinal;
            part.Dispose();
            return isFinal;
        }

        return false;
    }

    private class MultipartAsyncContext
    {
        public MultipartAsyncContext(Stream contentStream, MimeMultipartBodyPartParser mimeParser, byte[] data, ICollection<HttpContent> result)
        {
            Contract.Assert(contentStream != null);
            Contract.Assert(mimeParser != null);
            Contract.Assert(data != null);

            ContentStream = contentStream;
            Result = result;
            MimeParser = mimeParser;
            Data = data;
        }

        public Stream ContentStream { get; private set; }

        public ICollection<HttpContent> Result { get; private set; }

        public byte[] Data { get; private set; }

        public MimeMultipartBodyPartParser MimeParser { get; private set; }
    }
}