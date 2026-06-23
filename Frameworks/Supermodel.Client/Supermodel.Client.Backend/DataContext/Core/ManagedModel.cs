using Newtonsoft.Json;
using Supermodel.Client.Backend.Models;
using Supermodel.Encryptor;

namespace Supermodel.Client.Backend.DataContext.Core;

public class ManagedModel
{
    #region Constructors
    public ManagedModel(IModel originalModel)
    {
        Model = originalModel;
        OriginalModelId = originalModel.Id;
        UpdateHash();
    }
    #endregion

    #region Methods
    public void UpdateHash()
    {
        OriginalModelHash = JsonConvert.SerializeObject(Model).GetMD5Hash();
    }
    public bool NeedsUpdating()
    {
        return ForceUpdate || JsonConvert.SerializeObject(Model).GetMD5Hash() != OriginalModelHash;
    }
    #endregion

    #region Properties
    public IModel Model { get; set; }
    public long OriginalModelId { get; set; }
    protected string? OriginalModelHash { get; set; }
    public bool ForceUpdate { get; set; }
    #endregion
}