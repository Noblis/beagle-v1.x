using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.Models;

namespace Supermodel.Client.Backend.Repository;

public interface IDataRepo<TModel> where TModel : class, IModel, new()
{
    #region Reads
    Task<TModel> GetByIdAsync(long id);
    Task<TModel?> GetByIdOrDefaultAsync(long id);
    Task<List<TModel>> GetAllAsync(int? skip = null, int? take = null);
    Task<long> GetCountAllAsync(int? skip = null, int? take = null);
    #endregion

    #region Batch Reads
    void DelayedGetById(out DelayedModel<TModel> model, long id);
    void DelayedGetByIdOrDefault(out DelayedModel<TModel> model, long id);
    void DelayedGetAll(out DelayedModels<TModel> models);
    void DelayedGetCountAll(out DelayedCount count);
    #endregion

    #region Queries
    Task<List<TModel>> GetWhereAsync(object? searchBy, string? sortBy = null, int? skip = null, int? take = null);
    Task<long> GetCountWhereAsync(object? searchBy);
    #endregion

    #region Batch Queries
    void DelayedGetWhere(out DelayedModels<TModel> models, object? searchBy, string? sortBy = null, int? skip = null, int? take = null);
    void DelayedGetCountWhere(out DelayedCount count, object? searchBy);
    #endregion

    #region Writes
    void Add(TModel model);
    void Delete(TModel model);
    void ForceUpdate(TModel model);
    #endregion
}