using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class FeynmanEq2 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 2;
        var x2 = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = x1;
        inputs[1] = x2;

        // Checked    
        var result = MathF.Exp(-MathF.Pow(x1 / x2, 2) / 2f) / (MathF.Sqrt(2f * MathF.PI) * x2);
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["x1", "x2"];
    }



    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
