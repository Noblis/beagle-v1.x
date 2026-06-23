#nullable disable

using System.Collections;
using Validation;

namespace Supermodel.Client.Backend.UnitOfWork;

public static class ImmutableStack
{
    #region Static Methods
    public static ImmutableStack<T> Create<T>()
    {
        return ImmutableStack<T>.Empty;
    }
    public static ImmutableStack<T> Create<T>(T item)
    {
        return ImmutableStack<T>.Empty.Push(item);
    }
    public static ImmutableStack<T> CreateRange<T>(IEnumerable<T> items)
    {
        // ReSharper disable PossibleMultipleEnumeration
        Requires.NotNull(items);
        var immutableStack = ImmutableStack<T>.Empty;
        foreach (T obj in items) immutableStack = immutableStack.Push(obj);
        return immutableStack;
        // ReSharper restore PossibleMultipleEnumeration
    }
    public static ImmutableStack<T> Create<T>(params T[] items)
    {
        Requires.NotNull(items);
        var immutableStack = ImmutableStack<T>.Empty;
        foreach (T obj in items) immutableStack = immutableStack.Push(obj);
        return immutableStack;
    }
    public static IImmutableStack<T> Pop<T>(this IImmutableStack<T> stack, out T value)
    {
        Requires.NotNull(stack);
        value = stack.Peek();
        return stack.Pop();
    }
    #endregion
}

public sealed class ImmutableStack<T> : IImmutableStack<T>
{
    #region Embedded Types
    public struct Enumerator
    {
        private readonly ImmutableStack<T> _originalStack;
        private ImmutableStack<T> _remainingStack;

        public T Current
        {
            get
            {
                if (_remainingStack == null || _remainingStack.IsEmpty) throw new InvalidOperationException();
                return _remainingStack.Peek();
            }
        }

        internal Enumerator(ImmutableStack<T> stack)
        {
            Requires.NotNull(stack);
            _originalStack = stack;
            _remainingStack = null;
        }

        public bool MoveNext()
        {
            if (_remainingStack == null) _remainingStack = _originalStack;
            else if (!_remainingStack.IsEmpty) _remainingStack = _remainingStack.Pop();
            return !_remainingStack.IsEmpty;
        }
    }
    private class EnumeratorObject : IEnumerator<T>
    {
        private readonly ImmutableStack<T> _originalStack;
        private ImmutableStack<T> _remainingStack;
        private bool _disposed;

        public T Current
        {
            get
            {
                ThrowIfDisposed();
                if (_remainingStack == null || _remainingStack.IsEmpty) throw new InvalidOperationException();
                return _remainingStack.Peek();
            }
        }

        object IEnumerator.Current => Current;

        internal EnumeratorObject(ImmutableStack<T> stack)
        {
            Requires.NotNull(stack);
            _originalStack = stack;
        }

        public bool MoveNext()
        {
            ThrowIfDisposed();
            if (_remainingStack == null) _remainingStack = _originalStack;
            else if (!_remainingStack.IsEmpty) _remainingStack = _remainingStack.Pop();
            return !_remainingStack.IsEmpty;
        }

        public void Reset()
        {
            ThrowIfDisposed();
            _remainingStack = null;
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (!_disposed) return;
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
    #endregion

    #region Constructors
    private ImmutableStack(){}
    private ImmutableStack(T head, ImmutableStack<T> tail)
    {
        Requires.NotNull(tail);
        _head = head;
        _tail = tail;
    }
    #endregion

    #region Methods
    public ImmutableStack<T> Clear()
    {
        return Empty;
    }
    IImmutableStack<T> IImmutableStack<T>.Clear()
    {
        return Clear();
    }
        
    public T Peek()
    {
        if (IsEmpty) throw new InvalidOperationException("Unable to Peek when ImmutableStack is Empty");
        return _head;
    }
        
    public ImmutableStack<T> Push(T value)
    {
        return new ImmutableStack<T>(value, this);
    }
    IImmutableStack<T> IImmutableStack<T>.Push(T value)
    {
        return Push(value);
    }

    public ImmutableStack<T> Pop()
    {
        if (IsEmpty) throw new InvalidOperationException("Unable to Pop when ImmutableStack is Empty");
        return _tail;
    }
    public ImmutableStack<T> Pop(out T value)
    {
        value = Peek();
        return Pop();
    }
    IImmutableStack<T> IImmutableStack<T>.Pop()
    {
        return Pop();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new EnumeratorObject(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new EnumeratorObject(this);
    }

    public ImmutableStack<T> Reverse()
    {
        var immutableStack1 = Clear();
        for (var immutableStack2 = this; !immutableStack2.IsEmpty; immutableStack2 = immutableStack2.Pop())
        {
            immutableStack1 = immutableStack1.Push(immutableStack2.Peek());
        }
        return immutableStack1;
    }
    #endregion

    #region Fields and Properties
    private readonly T _head;
    private readonly ImmutableStack<T> _tail;
    public static ImmutableStack<T> Empty { get; } = new();
    public bool IsEmpty => _tail == null;
    #endregion
}