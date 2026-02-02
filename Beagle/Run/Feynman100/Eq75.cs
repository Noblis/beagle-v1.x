using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_75 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges: {1,5}
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var v = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = q;
        inputs[1] = v;
        inputs[2] = r;

        // f = q * v / (2 * pi * r)
        var result = (q * v) / (2f * MathF.PI * r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "q", "v", "r" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
