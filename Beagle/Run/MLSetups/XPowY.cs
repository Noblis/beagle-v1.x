using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class XPowY : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x = Rnd.Random.NextSingle() * 10;
        var y = Rnd.Random.NextSingle() * 5;

        allInputs[inputSetIdx, 0] = x;
        allInputs[inputSetIdx, 1] = y;
        
        return (allInputs, MathF.Pow(x, y));
    }
    public override string[] GetInputLabels()
    {
        return ["x", "y"];
    }

    public override int TargetColonySize(int generation)
    {
        // ReSharper disable once DuplicatedStatements
        //if (generation == 0) return 1_000_000;

        return 1_000_000;
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 100_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}