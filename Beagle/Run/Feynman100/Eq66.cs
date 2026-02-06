using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq66 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var e = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var c = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var l = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var r = Rnd.Random.NextSingle() * 4 + 1; // 1..5

        inputs[0]=e; inputs[1]=c; inputs[2]=l; inputs[3]=r;

        // f = (1 / (4*pi*e*c^2)) * (2*l / r)
        var result = (1 / (4 * MathF.PI * e * MathF.Pow(c,2))) * (2 * l / r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "e", "c", "l", "r" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
