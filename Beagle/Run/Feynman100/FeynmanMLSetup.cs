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
        var remainder = generation % 1500;
        if (remainder < 5) return 5_000_000;
        if (remainder < 10) return 3_000_000;
        if (remainder < 15) return 1_000_000;
        if (remainder < 20) return 500_000;
        return 250_000;
    }

    public sealed override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public sealed override double SolutionFoundASRThreshold => 1.0;
    public sealed override bool KeepOptimizingAfterSolutionFound => true;
    public sealed override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube && x != OpEnum.Pow).ToArray();
    #endregion
}