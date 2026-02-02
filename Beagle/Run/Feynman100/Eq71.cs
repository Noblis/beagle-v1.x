using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_71 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var q = Rnd.Random.NextSingle() * 4 + 1; // 
        var e = Rnd.Random.NextSingle() * 4 + 1; // 
        var r = Rnd.Random.NextSingle() * 4 + 1; // 
        var v = Rnd.Random.NextSingle() * 1 + 1; // 
        var c = Rnd.Random.NextSingle() * 7 + 3; //
        inputs[0] = q;
        inputs[1] = e;
        inputs[2] = r;
        inputs[3] = v;
        inputs[4] = c;
        var denom = (1 - v / c);
        var result = (q) / (4 * r * e * denom * MathF.PI);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "q", "e", "r", "v", "c" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
};
