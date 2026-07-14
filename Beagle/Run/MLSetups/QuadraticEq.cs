using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class QuadraticEq : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var a = Rnd.Random.NextSingle()*20 - 10;
        var b = Rnd.Random.NextSingle()*20 - 10;
        var c = Rnd.Random.NextSingle()*20 - 10;

        inputs[0] = a;
        inputs[1] = b;
        inputs[2] = c;

        var output = (-b + MathF.Sqrt(b*b - 4f*a*c)) / (2f*a);
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b", "c"];
    }

    public override double SolutionFoundASRThreshold => 1.0;
    public override long TotalBirthsToResetColonyIfNoProgress => 120_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}