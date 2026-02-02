using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_62 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var p = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var E = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var theta = Rnd.Random.NextSingle() * 2 + 1; // 1..3 (rad)
        var k = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var T = Rnd.Random.NextSingle() * 2 + 1; // 1..3

        inputs[0]=n; inputs[1]=p; inputs[2]=E; inputs[3]=theta; inputs[4]=k; inputs[5]=T;

        // f = n * (1 + (p * E * Cos[theta])/(k*T))
        var result = n * (1 + (p * E * MathF.Cos(theta)) / (k * T));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "n", "p", "E", "theta", "k", "T" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 300_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
