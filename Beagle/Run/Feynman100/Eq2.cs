using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class FeynmanEq2 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var θ = 1 + Rnd.Random.NextSingle() * 2;
        var σ = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = θ;
        inputs[1] = σ;

        // Checked    
        var result = MathF.Exp(-(θ / σ) * (θ / σ) / 2f) / (MathF.Sqrt(2f * MathF.PI) * σ);
        //var result = MathF.Exp(-MathF.Pow(x1 / x2, 2) / 2f) / (MathF.Sqrt(2f * MathF.PI) * x2);
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["x1", "x2"];
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube && x != OpEnum.Pow).ToArray();
    #endregion
}
