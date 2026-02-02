using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class FeynmanEq6 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 1;
        var x3 = 3 + Rnd.Random.NextSingle() * 7;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;

        var result = x1 / MathF.Sqrt(1f - MathF.Pow(x2, 2) / MathF.Pow(x3, 2));
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