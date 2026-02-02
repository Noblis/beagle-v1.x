using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_57 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
      
        var eps = Rnd.Random.NextSingle() * 2 + 1; // epsilon
        var p = Rnd.Random.NextSingle() * 2 + 1;
        var r = Rnd.Random.NextSingle() * 2 + 1;
        var theta = Rnd.Random.NextSingle() * 2 + 1; // theta in radians

        inputs[0] = eps;
        inputs[1] = p;
        inputs[2] = r;
        inputs[3] = theta;

        // f = (3/(4*pi*eps)) * p / ( r^3 * cos(theta) * sin(theta) )
        var numerator = 3;
        var denom1 = 4 * MathF.PI * eps;
        var denom2 = MathF.Pow(r, 3) * MathF.Cos(theta) * MathF.Sin(theta);
        var result = (numerator / denom1) * (p / denom2);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "eps", "p", "r", "theta" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
