using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using WebMonk.Context.WMHttpListenerObjects;
using WebMonk.Exceptions;
using WebMonk.Multipart;

namespace WebMonk.ValueProviders;

public class MessageBodyValueProvider : ValueProvider
{
    #region Methods
    public virtual async Task<IValueProvider> InitAsync(IHttpListenerRequest? request)
    {
        //this try/catch is a workaround to a likely bug in the framework
        try
        {
            if (request == null || !request.HasEntityBody) return this;
        }
        catch (NullReferenceException)
        {
            return this;
        }

        if (request.ContentType?.StartsWith("multipart/mixed") == true) return this;

        if (request.ContentType == "application/x-www-form-urlencoded")
        {
            await using(var inputStream = request.InputStream)
            {
                using (var streamReader = new StreamReader(inputStream, request.ContentEncoding))
                {
                    var dict = new Dictionary<string, object>();
                    var body = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    var pieces = body.Split('&').Select(x => x.Split('=')).ToArray();
                    foreach (var piece in pieces)
                    {
                        var key = HttpUtility.UrlDecode(piece[0]);
                        var value = HttpUtility.UrlDecode(piece[1]);
                        if (dict.ContainsKey(key))
                        {
                            var currentDictValue = dict[key];
                            if (currentDictValue is IList<string> list) list.Add(value);
                            else dict[key] = new List<string> { (string)currentDictValue, value };
                        }
                        else
                        {
                            dict.Add(key, value);
                        }
                    }
                    return await base.InitAsync(dict).ConfigureAwait(false);
                }
            }     
        }

        if (request.ContentType?.StartsWith("multipart/form-data") == true)
        {
            var streamContent = new StreamContent(request.InputStream);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);

            var dict = new Dictionary<string, object>();
            var provider = await streamContent.ReadAsMultipartAsync().ConfigureAwait(false);
            foreach (var httpContent in provider.Contents)
            {
                await using (var stream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var name = httpContent.Headers.ContentDisposition.Name.Replace("\"", "");
                    var fileName = httpContent.Headers.ContentDisposition.FileName;
                    //if (string.IsNullOrWhiteSpace(fileName)) //for .net 9.0 or below
                    if (fileName == null) //for .net10
                    {
                        //if field is not a file
                        using (var streamReader = new StreamReader(stream, request.ContentEncoding))
                        {
                            var key = name;
                            var value = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                            if (dict.ContainsKey(key))
                            {
                                var currentDictValue = dict[key];
                                if (currentDictValue is IList<string> list) list.Add(value);
                                else dict[key] = new List<string> { (string)currentDictValue, value };
                            }
                            else
                            {
                                dict.Add(key, value);
                            }
                        }
                    }
                    else
                    {
                        //if field is a file
                        using (var binaryReader = new BinaryReader(stream))
                        {
                            var key = name;
                            var value = binaryReader.ReadBytes((int)stream.Length);
                            if (dict.ContainsKey(key))
                            {
                                var currentDictValue = dict[key];
                                if (currentDictValue is IList<byte[]> list) list.Add(value);
                                else dict[key] = new List<byte[]> { (byte[])currentDictValue, value };
                            }
                            else
                            {
                                dict.Add(key, value);
                            }

                            var fileNameKey = $"{name}{IValueProvider.FileNameSuffix}";
                            var fileNameValue = fileName.Replace("\"", "");
                            if (dict.ContainsKey(fileNameKey))
                            {
                                var currentDictValue = dict[fileNameKey];
                                if (currentDictValue is IList<string> list) list.Add(fileNameValue);
                                else dict[fileNameKey] = new List<string> { (string)currentDictValue, fileNameValue };
                            }
                            else
                            {
                                dict.Add(fileNameKey, fileNameValue);
                            }
                        }
                    }
                }
            }
            return await base.InitAsync(dict).ConfigureAwait(false);
        }

        if (request.ContentType == "application/json" || request.ContentType == "application/json; charset=utf-8" ||
            request.ContentType == "application/xml" || request.ContentType == "application/xml; charset=utf-8" ||
            request.ContentType == "text/xml" || request.ContentType == "text/xml; charset=utf-8")
        {
            await using (var inputStream = request.InputStream)
            {
                using (var streamReader = new StreamReader(inputStream, request.ContentEncoding))
                {
                    var body = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    var dict = new Dictionary<string, object> { { "", body} };
                    return await base.InitAsync(dict).ConfigureAwait(false);
                }
            }
        }

        throw new Exception415UnsupportedMediaType(request.ContentType);
    }
    #endregion
}