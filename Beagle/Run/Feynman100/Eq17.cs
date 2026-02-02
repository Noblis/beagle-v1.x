using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class FeynmanEq17 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x = 5 + Rnd.Random.NextSingle() * 5;
        var u = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 17;
        var t = 1 + Rnd.Random.NextSingle() * 1;

        inputs[0] = x;
        inputs[1] = u;
        inputs[2] = c;
        inputs[3] = t;

        var result = (x - u * t) / MathF.Sqrt(1f - MathF.Pow(u, 2) / MathF.Pow(c, 2));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new string[] { "x", "u", "c", "t" };
    }



    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}