using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq60 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges: {1,5}
        var sigma = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var chi = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = sigma;
        inputs[1] = eps;
        inputs[2] = chi;

        // f = (sigma / eps) * (1 / (1 + chi))
        var result = (sigma / eps) * (1 / (1 + chi));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "sigma", "eps", "chi" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
