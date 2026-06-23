using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebMonk.Multipart;

public class MultipartMemoryStreamProvider : MultipartStreamProvider
{
    public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (headers == null) throw new ArgumentNullException(nameof(headers));
        return new MemoryStream();
    }
}