using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DemoForMSU : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = Rnd.Random.NextSingle() * 5;
        var x2 = Rnd.Random.NextSingle() * 5;
        var y1 = Rnd.Random.NextSingle() * 5;
        var y2 = Rnd.Random.NextSingle() * 5;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = y1;
        inputs[3] = y2;
        
        var result = MathF.Sqrt((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "y1", "y2"];
    }

    public override int TargetColonySize(int generation)
    {
        if (generation % 1000 < 15) return 5_000_000;
        return 500_000;
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}