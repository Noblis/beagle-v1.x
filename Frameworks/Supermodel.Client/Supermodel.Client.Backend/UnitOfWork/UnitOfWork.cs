using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.DataAnnotations.Exceptions;

namespace Supermodel.Client.Backend.UnitOfWork;

public class UnitOfWork<TDataContext> : IAsyncDisposable where TDataContext : class, IDataContext, new()
{
    #region Constructors
    public UnitOfWork(ReadOnly readOnly = ReadOnly.No)
    {
        Context = new TDataContext();
        if (readOnly == ReadOnly.Yes) Context.MakeReadOnly();
        UnitOfWorkContext<TDataContext>.PushDbContext(Context);
    }
    #endregion

    #region IDisposable implemetation
    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        var context = UnitOfWorkContext<TDataContext>.PopDbContext();
        if (context != Context) throw new SupermodelException("POP on Dispose popped mismatched Data Context.");
    }
    #endregion

    #region Properties
    public TDataContext Context { get; }
    #endregion
}