using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_64 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 1 + 0; // 0..1
        var alpha = Rnd.Random.NextSingle() * 1 + 0; // 0..1
        var e = Rnd.Random.NextSingle() * 1 + 1; // 1.2
        var E = Rnd.Random.NextSingle() * 1 + 1; // 1..2

        inputs[0]=n; inputs[1]=alpha; inputs[2]=e; inputs[3]=E;

        // f = (n*alpha / (1 - n*alpha/3)) * e * E
        var result = (n * alpha / (1f - (n * alpha / 3f))) * e * E;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "n", "alpha", "e", "E" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
