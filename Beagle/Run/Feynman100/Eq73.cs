using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_73 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges: {1,5} for each
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var c = Rnd.Random.NextSingle() * 4 + 1;
        var E = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = eps;
        inputs[1] = c;
        inputs[2] = E;

        // f = eps * c * E^2
        var result = eps * c * E * E;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "eps", "c", "E" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
