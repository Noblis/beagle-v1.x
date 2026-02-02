using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class FeynmanEq18 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var t = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;
        var x = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = t;
        inputs[1] = u;
        inputs[2] = c;
        inputs[3] = x;
        // Checked
        var result = (t - u * x / MathF.Pow(c, 2)) / MathF.Sqrt(1f - MathF.Pow(u, 2) / MathF.Pow(c, 2));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new string[] { "t", "u", "c", "x" };
    }



    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}