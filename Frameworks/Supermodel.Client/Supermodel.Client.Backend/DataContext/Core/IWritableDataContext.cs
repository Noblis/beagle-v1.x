using Supermodel.Client.Backend.Models;

namespace Supermodel.Client.Backend.DataContext.Core;

public interface IWritableDataContext : IDataContext
{
    #region Writes
    void Add<TModel>(TModel model) where TModel : class, IModel, new();
    void Delete<TModel>(TModel model) where TModel : class, IModel, new();
    void ForceUpdate<TModel>(TModel model) where TModel : class, IModel, new();
    void DetectAllUpdates();
    #endregion

    #region Save Changes
    Task SaveChangesAsync();
    Task FinalSaveChangesAsync();
    #endregion
}