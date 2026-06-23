using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.DataAnnotations.Validations;

namespace Supermodel.Client.Backend.Models;

public interface IModel : IAsyncValidatableObject
{
    long Id { get; set; }
    bool IsNew { get; }

    DateTime? BroughtFromMasterDbOnUtc { get; set; }

    void Add();
    void Delete();
    void Update();

    void BeforeSave(PendingAction.OperationEnum operation);
    void AfterLoad();
}