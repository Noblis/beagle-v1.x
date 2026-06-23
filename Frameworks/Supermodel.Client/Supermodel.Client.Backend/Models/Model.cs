using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Supermodel.Client.Backend.DataContext.Core;
using Supermodel.Client.Backend.DataContext.Sqlite;
using Supermodel.Client.Backend.Repository;
using Supermodel.Client.Backend.UnitOfWork;
using Supermodel.DataAnnotations.Validations;
using Supermodel.ReflectionMapper;

namespace Supermodel.Client.Backend.Models;

public abstract class Model : IModel
{
    #region Methods
    public virtual void Add()
    {
        CreateRepo().ExecuteMethod("Add", this);
    }
    public virtual void Delete()
    {
        CreateRepo().ExecuteMethod("Delete", this);
    }
    public virtual void Update()
    {
        CreateRepo().ExecuteMethod("ForceUpdate", this);
    }

    public virtual void BeforeSave(PendingAction.OperationEnum operation)
    {
        //default is doing nothing
    }
    public virtual void AfterLoad()
    {
        //default is doing nothing
    }

    public virtual object CreateRepo()
    {
        return RepoFactory.CreateForRuntimeType(GetType());
    }
    public Task<ValidationResultList> ValidateAsync(ValidationContext validationContext)
    {
        return Task.FromResult(new ValidationResultList());
    }
    #endregion

    #region Properties
    public long Id { get; set; }
    [JsonIgnore, NotRMapped] public virtual bool IsNew => Id == 0;
    //[NotRMapped, NotRCompared, JsonIgnore] public virtual string Identity => Id.ToString();

    [NotRMapped] public DateTime? BroughtFromMasterDbOnUtc { get; set; }
    public bool ShouldSerializeBroughtFromMasterDbOnUtc()
    {
        if (UnitOfWorkContextCore.StackCount == 0) return false;
        return UnitOfWorkContextCore.CurrentDataContext is SqliteDataContext;
    }
    #endregion
}