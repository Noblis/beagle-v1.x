using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq69 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var u = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var B = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var theta = Rnd.Random.NextSingle() * 4 + 1; // 1..5 rad

        inputs[0]=u; inputs[1]=B; inputs[2]=theta;

        // f = -u * B * Cos(theta)
        var result = -u * B * MathF.Cos(theta);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "u", "B", "theta" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
