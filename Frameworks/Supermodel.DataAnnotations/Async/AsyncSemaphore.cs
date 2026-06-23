#nullable disable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Supermodel.DataAnnotations.Async;

public class AsyncSemaphore
{
    #region Methods
    public AsyncSemaphore(int initialCount)
    {
        if (initialCount < 0) throw new ArgumentOutOfRangeException(nameof(initialCount));
        _currentCount = initialCount;
    }
    public Task WaitAsync()
    {
        lock (_waiters)
        {
            if (_currentCount > 0)
            {
                --_currentCount;
                return _completed;
            }
            else
            {
                var waiter = new TaskCompletionSource<bool>();
                _waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }
    }
    public void Release()
    {
        TaskCompletionSource<bool> toRelease = null;
        lock (_waiters)
        {
            if (_waiters.Count > 0) toRelease = _waiters.Dequeue();
            else ++_currentCount;
        }
        if (toRelease != null) toRelease.SetResult(true);
    }
    #endregion

    #region Attributes
    private static readonly Task _completed = Task.FromResult(true);
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private int _currentCount;
    #endregion
}