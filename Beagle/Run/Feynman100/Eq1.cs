using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class Eq1 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var θ = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = θ;

        // Checked
        var result = MathF.Exp(-θ * θ / 2f) / MathF.Sqrt(2f * MathF.PI);
        //var result = MathF.Exp(-MathF.Pow(x1, 2) / 2f) / MathF.Sqrt(2f * MathF.PI);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["θ"];
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube).ToArray();
    #endregion
}