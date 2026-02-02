using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_65 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 1 + 0; // 0..1
        var alpha = Rnd.Random.NextSingle() * 1 + 0; // 1..3

        inputs[0]=n; inputs[1]=alpha;

        // f = 1 + n*alpha / (1 - (n*alpha/3))
        var result = 1 + (n * alpha) / (1 - (n * alpha / 3f));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "n", "alpha" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
