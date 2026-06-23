using Supermodel.Client.Backend.Models;

namespace Supermodel.Client.Backend.Repository;

public interface IRepoFactory
{
    IDataRepo<TModel>? CreateRepo<TModel>() where TModel : class, IModel, new();
}