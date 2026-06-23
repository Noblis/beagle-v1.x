#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebMonk.Multipart;

public class MultipartContent : HttpContent, IEnumerable<HttpContent>
{
    private readonly List<HttpContent> _nestedContent;
    private readonly string _boundary;
    private int _nextContentIndex;
    private Stream _outputStream;
    private TaskCompletionSource<object> _tcs;
    //private const string crlf = "\r\n";

    public MultipartContent() : this("mixed", GetDefaultBoundary()){}

    public MultipartContent(string subtype) : this(subtype, GetDefaultBoundary()) {}

    public MultipartContent(string subtype, string boundary)
    {
        if (string.IsNullOrWhiteSpace(subtype)) throw new ArgumentException("subtype");
        ValidateBoundary(boundary);
        _boundary = boundary;
        var str = boundary;
        if (!str.StartsWith("\"", StringComparison.Ordinal)) str = "\"" + str + "\"";
        Headers.ContentType = new MediaTypeHeaderValue("multipart/" + subtype)
        {
            Parameters = { new NameValueHeaderValue("boundary", str) }
        };
        _nestedContent = [];
    }

    public virtual void Add(HttpContent content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        _nestedContent.Add(content);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (HttpContent httpContent in _nestedContent) httpContent.Dispose();
            _nestedContent.Clear();
        }
        base.Dispose(disposing);
    }

    public IEnumerator<HttpContent> GetEnumerator()
    {
        return _nestedContent.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _nestedContent.GetEnumerator();
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        TaskCompletionSource<object> completionSource = new TaskCompletionSource<object>();
        _tcs = completionSource;
        _outputStream = stream;
        _nextContentIndex = 0;
        EncodeStringToStreamAsync(_outputStream, "--" + _boundary + "\r\n").ContinueWithStandard(WriteNextContentHeadersAsync);
        return completionSource.Task;
    }

    protected override bool TryComputeLength(out long length)
    {
        const long num1 = 0L;
        var num2 = (long) GetEncodedLength("\r\n--" + _boundary + "\r\n");
        var num3 = num1 + GetEncodedLength("--" + _boundary + "\r\n");
        var flag = true;
        foreach (var httpContent in _nestedContent)
        {
            if (flag) flag = false;
            else num3 += num2;

            foreach (var keyValuePair in httpContent.Headers) num3 += GetEncodedLength(keyValuePair.Key + ": " + string.Join(", ", keyValuePair.Value) + "\r\n");
            num3 += "\r\n".Length;
            
            // ReSharper disable once ConvertToConstant.Local
            var length1 = 0L;
            var methodInfo = httpContent.GetType().GetMethod("TryComputeLength", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (bool)methodInfo!.Invoke(httpContent, [ /*out*/length1]); 
            if (result)
            {
                length = 0L;
                return false;
            }

            num3 += length1;
        }
        var num4 = num3 + GetEncodedLength("\r\n--" + _boundary + "--\r\n");
        length = num4;
        return true;
    }

    private static void ValidateBoundary(string boundary)
    {
        if (string.IsNullOrWhiteSpace(boundary)) throw new ArgumentException("boundary");
        if (boundary.Length > 70) throw new ArgumentOutOfRangeException(nameof(boundary), "boundary is longer than 70 characters");
        if (boundary.EndsWith(" ", StringComparison.Ordinal)) throw new ArgumentException("boundary ends with blank", nameof(boundary));
        if (boundary.Any(ch => (48 > ch || ch > 57) && (97 > ch || ch > 122) && ((65 > ch || ch > 90) && "'()+_,-./:=? ".IndexOf(ch) < 0))) throw new ArgumentException("boundary contains invalid characters", nameof(boundary));
    }

    private static string GetDefaultBoundary()
    {
        return Guid.NewGuid().ToString();
    }

    private void WriteNextContentHeadersAsync(Task task)
    {
        if (task.IsFaulted)
        {
            // ReSharper disable once PossibleNullReferenceException
            HandleAsyncException(task.Exception.GetBaseException());
        }
        else
        {
            try
            {
                if (_nextContentIndex >= _nestedContent.Count)
                {
                    WriteTerminatingBoundaryAsync();
                }
                else
                {
                    var str = "\r\n--" + _boundary + "\r\n";
                    var stringBuilder = new StringBuilder();
                    if (_nextContentIndex != 0) stringBuilder.Append(str);
                    foreach (var keyValuePair in _nestedContent[_nextContentIndex].Headers) stringBuilder.Append(keyValuePair.Key + ": " + string.Join(", ", keyValuePair.Value) + "\r\n");
                    stringBuilder.Append("\r\n");
                    EncodeStringToStreamAsync(_outputStream, stringBuilder.ToString()).ContinueWithStandard(WriteNextContentAsync);
                }
            }
            catch (Exception ex)
            {
                HandleAsyncException(ex);
            }
        }
    }

    private void WriteNextContentAsync(Task task)
    {
        if (task.IsFaulted)
        {
            // ReSharper disable once PossibleNullReferenceException
            HandleAsyncException(task.Exception.GetBaseException());
        }
        else
        {
            try
            {
                var httpContent = _nestedContent[_nextContentIndex];
                ++_nextContentIndex;
                httpContent.CopyToAsync(_outputStream).ContinueWithStandard(WriteNextContentHeadersAsync);
            }
            catch (Exception ex)
            {
                HandleAsyncException(ex);
            }
        }
    }

    private void WriteTerminatingBoundaryAsync()
    {
        try
        {
            EncodeStringToStreamAsync(_outputStream, "\r\n--" + _boundary + "--\r\n").ContinueWithStandard(task =>
            {
                // ReSharper disable once PossibleNullReferenceException
                if (task.IsFaulted) HandleAsyncException(task.Exception.GetBaseException());
                else CleanupAsync().TrySetResult(null);
            });
        }
        catch (Exception ex)
        {
            HandleAsyncException(ex);
        }
    }

    private static Task EncodeStringToStreamAsync(Stream stream, string input)
    {
        var bytes = Encoding.GetEncoding(28591).GetBytes(input);
        //return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, bytes, 0, bytes.Length, null); 
        return stream.WriteAsync(bytes, 0, bytes.Length); //Changed to accomodate WinRT
    }

    private TaskCompletionSource<object> CleanupAsync()
    {
        var completionSource = _tcs;
        _outputStream = null;
        _nextContentIndex = 0;
        _tcs = null;
        return completionSource;
    }

    private void HandleAsyncException(Exception ex)
    {
        CleanupAsync().TrySetException(ex);
    }

    private static int GetEncodedLength(string input)
    {
        return Encoding.GetEncoding(28591).GetByteCount(input);
    }
}