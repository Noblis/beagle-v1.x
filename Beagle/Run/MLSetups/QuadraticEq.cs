using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class QuadraticEq : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var a = Rnd.Random.NextSingle()*20 - 10;
        var b = Rnd.Random.NextSingle()*20 - 10;
        var c = Rnd.Random.NextSingle()*20 - 10;

        allInputs[inputSetIdx, 0] = a;
        allInputs[inputSetIdx, 1] = b;
        allInputs[inputSetIdx, 2] = c;

        var output = (-b + MathF.Sqrt(b*b - 4f*a*c)) / (2f*a);
        return (allInputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b", "c"];
    }

    public override double SolutionFoundASRThreshold => 1.0;
    public override long TotalBirthsToResetColonyIfNoProgress => 1_200_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}