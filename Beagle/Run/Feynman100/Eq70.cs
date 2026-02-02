using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_70 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var p = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var E = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var theta = Rnd.Random.NextSingle() * 4 + 1; // 1..5 rad

        inputs[0]=p; inputs[1]=E; inputs[2]=theta;

        // f = -p * E * Cos(theta)
        var result = -p * E * MathF.Cos(theta);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "p", "E", "theta" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
