using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_72 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges shown: omega {2,4}, c {4,6}, d {1,2}
        var omega = Rnd.Random.NextSingle() * 2 + 2;
        var c = Rnd.Random.NextSingle() * 2 + 4;
        var d = Rnd.Random.NextSingle() * 1 + 1;

        inputs[0] = omega;
        inputs[1] = c;
        inputs[2] = d;

        // f = sqrt( omega^2 / ( c^2 - pi^2 / d^2 ) )
        var denom = c * c - (MathF.PI * MathF.PI) / (d * d);
        var result = MathF.Sqrt( (omega * omega) / denom );
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "omega", "c", "d" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
