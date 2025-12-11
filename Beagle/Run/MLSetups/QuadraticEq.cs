using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class QuadraticEq : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var a = Rnd.Random.NextSingle()*200 - 100;
        var b = Rnd.Random.NextSingle()*200 - 100;
        var c = Rnd.Random.NextSingle()*200 - 100;
        inputs[0] = a;
        inputs[1] = b;
        inputs[2] = c;
        var output = (-b + MathF.Sqrt(b*b - 4*a*c)) / (2*a);
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b", "c"];
    }

    public override int TargetColonySize(int generation)
    {
        if (generation % 1500 < 20) return 5_000_000;
        return 500_000;
    }

    public override double SolutionFoundASRThreshold => 1.0;

    public override long TotalBirthsToResetColonyIfNoProgress => 600_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}