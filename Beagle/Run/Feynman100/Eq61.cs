using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq61 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var q = Rnd.Random.NextSingle() * 2 + 1; // range 1..3
        var E = Rnd.Random.NextSingle() * 2 + 1; // range 1..3
        var m = Rnd.Random.NextSingle() * 2 + 1; // range 3..5
        var w0 = Rnd.Random.NextSingle() * 2 + 3; // range 1..2
        var w = Rnd.Random.NextSingle() * 1 + 1; // range 1..2

        inputs[0] = q;
        inputs[1] = E;
        inputs[2] = m;
        inputs[3] = w0;
        inputs[4] = w;

        // f = (q * E) / ( m * (w0^2 + w^2) )
        var result = (q * E) / (m * (MathF.Pow(w0,2) + MathF.Pow(w,2)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "q", "E", "m", "w0", "w" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
