using Supermodel.Client.Backend.Models;

namespace Supermodel.Client.Backend.DataContext.Core;

public interface IReadableDataContext : IDataContext
{
    #region Reads
    Task<TModel> GetByIdAsync<TModel>(long id) where TModel : class, IModel, new();
    Task<TModel?> GetByIdOrDefaultAsync<TModel>(long id) where TModel : class, IModel, new();
    Task<List<TModel>> GetAllAsync<TModel>(int? skip = null, int? take = null) where TModel : class, IModel, new();
    Task<long> GetCountAllAsync<TModel>(int? skip = null, int? take = null) where TModel : class, IModel, new();
    #endregion

    #region Batch Reads
    void DelayedGetById<TModel>(out DelayedModel<TModel> model, long id) where TModel : class, IModel, new();
    void DelayedGetByIdOrDefault<TModel>(out DelayedModel<TModel> model, long id) where TModel : class, IModel, new();
    void DelayedGetAll<TModel>(out DelayedModels<TModel> models) where TModel : class, IModel, new();
    void DelayedGetCountAll<TModel>(out DelayedCount count) where TModel : class, IModel, new();
    #endregion
}