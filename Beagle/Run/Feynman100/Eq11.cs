using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class FeynmanEq11 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var x3 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;

        var result = (x1 * x2) / (4f * MathF.PI * x3 * MathF.Pow(x2, 3));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new string[] { "x1", "x2", "x3" };
    }



    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}