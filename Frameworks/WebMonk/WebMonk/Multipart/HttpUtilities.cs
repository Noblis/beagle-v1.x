using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebMonk.Multipart;

public static class HttpUtilities
{
    //internal static readonly Version DefaultVersion = HttpVersion.Version11;
    // ReSharper disable once InconsistentNaming
    internal static readonly byte[] EmptyByteArray = [];

    static HttpUtilities() { }

    public static bool IsHttpUri(Uri uri)
    {
        var scheme = uri.Scheme;
        if (string.Compare("http", scheme, StringComparison.OrdinalIgnoreCase) != 0) return string.Compare("https", scheme, StringComparison.OrdinalIgnoreCase) == 0;
        else return true;
    }

    public static bool HandleFaultsAndCancellation<T>(Task task, TaskCompletionSource<T> tcs)
    {
        if (task.IsFaulted)
        {
            // ReSharper disable once PossibleNullReferenceException
            tcs.TrySetException(task.Exception.GetBaseException());
            return true;
        }

        if (!task.IsCanceled) return false;
        tcs.TrySetCanceled();
        return true;
    }

    public static Task ContinueWithStandard(this Task task, Action<Task> continuation)
    {
        return task.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    public static Task ContinueWithStandard<T>(this Task<T> task, Action<Task<T>> continuation)
    {
        return task.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
}