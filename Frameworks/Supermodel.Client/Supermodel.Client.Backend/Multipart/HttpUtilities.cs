#nullable disable

namespace Supermodel.Client.Backend.Multipart;

public static class HttpUtilities
{
    //internal static readonly Version DefaultVersion = HttpVersion.Version11;
    internal static readonly byte[] EmptyByteArray = Array.Empty<byte>();

    static HttpUtilities() { }

    public static bool IsHttpUri(Uri uri)
    {
        var scheme = uri.Scheme;
        if (string.Compare("http", scheme, StringComparison.OrdinalIgnoreCase) != 0) return string.Compare("https", scheme, StringComparison.OrdinalIgnoreCase) == 0;
        else return true;
    }

    public static bool HandleFaultsAndCancelation<T>(Task task, TaskCompletionSource<T> tcs)
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