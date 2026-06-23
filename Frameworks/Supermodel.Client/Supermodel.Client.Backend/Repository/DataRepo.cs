using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.Models;
using Supermodel.Client.Backend.UnitOfWork;
using Supermodel.DataAnnotations.Exceptions;

namespace Supermodel.Client.Backend.Repository;

public class DataRepo<TModel> : IDataRepo<TModel> where TModel : class, IModel, new()
{
    #region Reads
    public virtual Task<TModel> GetByIdAsync(long id)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support GetByIdAsync operation");
        return ((IReadableDataContext) context).GetByIdAsync<TModel>(id);
    }
    public virtual Task<TModel?> GetByIdOrDefaultAsync(long id)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support GetByIdOrDefaultAsync operation");
        return ((IReadableDataContext) context).GetByIdOrDefaultAsync<TModel>(id);
    }
    public virtual Task<List<TModel>> GetAllAsync(int? skip = null, int? take = null)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support GetAllAsync operation");
        return ((IReadableDataContext) context).GetAllAsync<TModel>(skip, take);
    }
    public virtual Task<long> GetCountAllAsync(int? skip = null, int? take = null)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support GetCountAllAsync operation");
        return ((IReadableDataContext) context).GetCountAllAsync<TModel>(skip, take);
    }
    #endregion

    #region Batch Reads
    public virtual void DelayedGetById(out DelayedModel<TModel> model, long id)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support DelayedGetById operation");
        ((IReadableDataContext) context).DelayedGetById(out model, id);
    }
    public virtual void DelayedGetByIdOrDefault(out DelayedModel<TModel> model, long id)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support DelayedGetByIdOrDefault operation");
        ((IReadableDataContext) context).DelayedGetByIdOrDefault(out model, id);
    }
    public virtual void DelayedGetAll(out DelayedModels<TModel> models)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support DelayedGetAll operation");
        ((IReadableDataContext) context).DelayedGetAll(out models);
    }
    public virtual void DelayedGetCountAll(out DelayedCount count)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IReadableDataContext)) throw new SupermodelException("Current DataContext does not support DelayedGetCountAll operation");
        ((IReadableDataContext) context).DelayedGetCountAll<TModel>(out count);
    }
    #endregion

    #region Queries
    public virtual Task<List<TModel>> GetWhereAsync(object? searchBy, string? sortBy = null, int? skip = null, int? take = null)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IQueryableReadableDataContext)) throw new SupermodelException("Current DataContext does not support GetWhereAsync operation");
        return ((IQueryableReadableDataContext) context).GetWhereAsync<TModel>(searchBy, sortBy, skip, take);
    }
    public virtual Task<long> GetCountWhereAsync(object? searchBy)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IQueryableReadableDataContext)) throw new SupermodelException("Current DataContext does not support GetCountWhereAsync operation");
        return ((IQueryableReadableDataContext) context).GetCountWhereAsync<TModel>(searchBy);
    }
    #endregion

    #region Delayed Queries
    public virtual void DelayedGetWhere(out DelayedModels<TModel> models, object? searchBy, string? sortBy = null, int? skip = null, int? take = null)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IQueryableReadableDataContext)) throw new SupermodelException("Current DataContext does not support DelayedGetWhere operation");
        ((IQueryableReadableDataContext) context).DelayedGetWhere(out models, searchBy, sortBy, skip, take);
    }
    public virtual void DelayedGetCountWhere(out DelayedCount count, object? searchBy)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IQueryableReadableDataContext)) throw new SupermodelException("Current DataContext does not support DelayedGetCountWhere operation");
        ((IQueryableReadableDataContext) context).DelayedGetCountWhere<TModel>(out count, searchBy);
    }
    #endregion

    #region Writes
    public virtual void Add(TModel model)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IWritableDataContext)) throw new SupermodelException("Current DataContext does not support Add operation");
        ((IWritableDataContext) context).Add(model);
    }
    public virtual void Delete(TModel model)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IWritableDataContext)) throw new SupermodelException("Current DataContext does not support Delete operation");
        ((IWritableDataContext) context).Delete(model);
    }
    public virtual void ForceUpdate(TModel model)
    {
        var context = UnitOfWorkContextCore.CurrentDataContext;
        if (!(context is IWritableDataContext)) throw new SupermodelException("Current DataContext does not support ForceUpdate operation");
        ((IWritableDataContext) context).ForceUpdate(model);
    }
    #endregion
}