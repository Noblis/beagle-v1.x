using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DepressedCubicEq : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var p = Rnd.Random.NextSingle() * 200 - 100;
        var q = Rnd.Random.NextSingle() * 200 - 100;

        allInputs[inputSetIdx, 0] = p;
        allInputs[inputSetIdx, 1] = p;

        var c = -q / 2;
        var d = MathF.Sqrt(q * q / 4 + p * p * p / 27);

        var answer = MathF.Cbrt(c + d) + MathF.Cbrt(c - d);

        return (allInputs, answer);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "q"];
    }
    public override int TargetColonySize(int generation)
    {
        if (generation % 1000 < 20) return 50_000_000;
        return 2_000_000;
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_000_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override double SolutionFoundASRThreshold => 1.0;
    public override uint ExperimentsPerGeneration => 1024;
    #endregion
}