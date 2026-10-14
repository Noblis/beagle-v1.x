using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DemoForMSU : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x1 = Rnd.Random.NextSingle() * 5;
        var x2 = Rnd.Random.NextSingle() * 5;
        var y1 = Rnd.Random.NextSingle() * 5;
        var y2 = Rnd.Random.NextSingle() * 5;

        allInputs[inputSetIdx, 0] = x1;
        allInputs[inputSetIdx, 1] = x2;
        allInputs[inputSetIdx, 2] = y1;
        allInputs[inputSetIdx, 3] = y2;
        
        var result = MathF.Sqrt((x2-x1)*(x2-x1) + (y2-y1)*(y2-y1));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "y1", "y2"];
    }

    //public override int TargetColonySize(int generation)
    //{
    //    if (generation % 1000 < 15) return 5_000_000;
    //    return 500_000;
    //}

    public override long TotalBirthsToResetColonyIfNoProgress => 500_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}