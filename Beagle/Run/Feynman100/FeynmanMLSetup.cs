using BeagleLib.Engine;
using BeagleLib.VM;

namespace Run.Feynman100;

public abstract class FeynmanMLSetup : MLSetup
{
    #region Overrides
    //use point-to-point fit func
    //public sealed override int TargetColonySize(int generation)
    //{
    //    if (generation % 1500 < 20) return 10_000_000;
    //    return 1_000_000;
    //}

    //use for correlation fit func
    public sealed override int TargetColonySize(int generation)
    {
        var remainder = generation % 525;
        if (remainder < 25) return 5_000_000;
        return 250_000;
    }

    public sealed override long TotalBirthsToResetColonyIfNoProgress => 750_000_000;
    public sealed override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube && x != OpEnum.Pow).ToArray();
    #endregion
}