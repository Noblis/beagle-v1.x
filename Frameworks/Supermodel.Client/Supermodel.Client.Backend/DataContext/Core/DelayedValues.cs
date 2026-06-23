using Supermodel.Client.Backend.Models;

namespace Supermodel.Client.Backend.DataContext.Core;

public abstract class DelayedValue
{
    public abstract void SetValue(object? value);
    public abstract object? GetValue();
}
    
public class DelayedModel<TModel> : DelayedValue where TModel : class, IModel, new() 
{
    #region Overrides
    public override void SetValue(object? value)
    {
        Value = (TModel?)value;
    }

    public override object? GetValue()
    {
        return Value;
    }
    #endregion

    #region Properties
    public TModel? Value { get; set; }
    #endregion
}

public class DelayedModels<TModel> : DelayedValue where TModel : class, IModel, new()
{
    #region Overrides
    public override void SetValue(object? value)
    {
        Values = (List<TModel>?)value;
    }
    public override object? GetValue()
    {
        return Values;
    }
    #endregion

    #region Properties
    public List<TModel>? Values { get; set; }
    #endregion
}

public class DelayedCount : DelayedValue
{
    #region Overrides
    public override void SetValue(object? value)
    {
        Value = (long?) value;
    }
    public override object? GetValue()
    {
        return Value;
    }
    #endregion

    #region Properties
    public long? Value { get; set; }
    #endregion
}