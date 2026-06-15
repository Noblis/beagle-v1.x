using BeagleLib.Util;

namespace BeagleLib.Engine;

public abstract class MLSetupV16 : MLSetup
{
    public override int TargetColonySize(int generation)
    {
        if (generation % 1500 < 20) return 10_000_000;
        return 1_000_000;
    }
    protected override int ScriptLengthTaxRateInternal => BConfig.MaxScore * (int)ExperimentsPerGeneration / 200;
}