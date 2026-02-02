using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_74 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges: {1,5} for eps and E
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var E = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = eps;
        inputs[1] = E;

        // f = eps * E^2
        var result = eps * E * E;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "eps", "E" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
