using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WebMonk.Context;

namespace WebMonk.HttpRequestHandlers;

public class StaticContentHttpRequestHandler : IHttpRequestHandler
{
    #region Embedded Types
    protected class CachedFile
    {
        #region Constructors
        public CachedFile(byte[] fileContent)
        {
            FileContent = fileContent;
            LastAccessed = DateTime.Now;
        }
        #endregion

        #region Methods
        public void UpdateLastAccessed()
        {
            LastAccessed = DateTime.Now;
        }
        #endregion

        #region Properties
        public byte[] FileContent { get; }
        public DateTime LastAccessed { get; protected set; }
        #endregion
    }
    #endregion
        
    #region Overrides
    public virtual int Priority => 200;
    public virtual bool SaveSessionState => false;
    public virtual async Task<IHttpRequestHandler.HttpRequestHandlerResult> TryExecuteHttpRequestAsync(CancellationToken cancellationToken)
    {
        var execDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var localPath = $"wwwroot{HttpContext.Current.RouteManager.LocalPath}";
        var fullPath = Path.Combine(execDir, localPath);
            
        var file = CachedFiles.GetValueOrDefault(localPath);
        if (file == null)
        {
            if (!File.Exists(fullPath)) return IHttpRequestHandler.HttpRequestHandlerResult.False;
            file = new CachedFile(await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false));

            CachedFiles[localPath] = file;
            if (CachedFiles.Count > MaxNumberOfFilesInCache) RemoveOldestCacheElement();
        }
        else
        {
            file.UpdateLastAccessed();
        }
        return new IHttpRequestHandler.HttpRequestHandlerResult(true, async () => 
        {
            HttpContext.Current.HttpListenerContext.Response.AddHeader("Content-Type", MimeTypes.GetMimeType(Path.GetFileName(localPath)));
            //HttpContext.Current.HttpListenerContext.Response.AddHeader("Content-Disposition", $"Attachment; filename=\"{Path.GetFileName(localPath)}\"");
            await HttpContext.Current.HttpListenerContext.Response.OutputStream.WriteAsync(file.FileContent, 0, file.FileContent.Length, cancellationToken).ConfigureAwait(false);
        });
    }
    protected void RemoveOldestCacheElement()
    {
        if (CachedFiles.IsEmpty) return;

        //find the file in cache that was accessed the longest time ago and try to delete it
        //we say FirstOrDefault instead of SingleOrDefault because sometimes we end up with multiple files with identical timestamp
        var fileToDelete = CachedFiles.FirstOrDefault(x => x.Value.LastAccessed == CachedFiles.Min(y => y.Value.LastAccessed));
        
        //if (!fileToDelete.Equals(default(KeyValuePair<string, CachedFile>))) //this is very inefficient, so we just check for key
        if (fileToDelete.Key != null)
        {
            if (!CachedFiles.TryRemove(fileToDelete.Key, out _)) RemoveOldestCacheElement();
        }
    }
    #endregion

    #region Properties
    protected ConcurrentDictionary<string, CachedFile> CachedFiles { get; } = new();
    public static int MaxNumberOfFilesInCache { get; set; } = 32;
    #endregion
}