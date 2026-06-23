#nullable disable

using System.Net.Http.Headers;

namespace Supermodel.Client.Backend.Multipart;

public class MultipartMemoryStreamProvider : MultipartStreamProvider
{
    public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        if (headers == null) throw new ArgumentNullException(nameof(headers));
        return new MemoryStream();
    }
}