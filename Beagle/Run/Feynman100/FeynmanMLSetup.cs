using BeagleLib.Engine;
using BeagleLib.VM;

namespace Run.Feynman100;

public abstract class FeynmanMLSetup : MLSetup
{
    #region Overrides
    //Comment out TargetColonySize when using correlation fit func, uncomment when using point-to-point fit func
    //public override int TargetColonySize(int generation)
    //{
    //    if (generation % 1500 < 20) return 10_000_000;
    //    return 1_000_000;
    //}

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube && x != OpEnum.Pow).ToArray();
    #endregion
}