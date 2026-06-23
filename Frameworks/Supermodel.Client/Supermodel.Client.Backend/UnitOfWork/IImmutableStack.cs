namespace Supermodel.Client.Backend.UnitOfWork;

public interface IImmutableStack<T> : IEnumerable<T>
{
    bool IsEmpty { get; }
    IImmutableStack<T> Clear();
    IImmutableStack<T> Push(T value);
    IImmutableStack<T> Pop();
    T Peek();
}