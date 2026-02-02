using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq67 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var p = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var v = Rnd.Random.NextSingle() * 1 + 1; // 0..1 (fraction of c)
        var c = Rnd.Random.NextSingle() * 7 + 3; // 3..10

        inputs[0]=p; inputs[1]=v; inputs[2]=c;

        // f = p / Sqrt(1 - v^2 / c^2)
        var result = p / MathF.Sqrt(1 - MathF.Pow(v,2) / MathF.Pow(c,2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "p", "v", "c" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
