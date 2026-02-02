using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class FeynmanEq19 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;

        inputs[0] = m;
        inputs[1] = v;
        inputs[2] = c;

        var result = (m * v) / MathF.Sqrt(1f - MathF.Pow(v, 2) / MathF.Pow(c, 2));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new string[] { "m", "v", "c" };
    }



    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}