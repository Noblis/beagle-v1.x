using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DemoForJdAndTheBoys : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var a = Rnd.Random.NextSingle() * 10 - 5;
        var b = Rnd.Random.NextSingle() * 10 - 5;

        allInputs[inputSetIdx, 0] = a;
        allInputs[inputSetIdx, 1] = b;
        
        var output = 3 * a * a * MathF.Sin(b) + 1 / (a * a);
        
        return (allInputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b"];
    }
    public override double SolutionFoundASRThreshold => 1.0;

    public override int TargetColonySize(int generation)
    {
        if (generation == 0) return 100_000_000;
        //if (generation <= 10) return 100_000_000;
        return 10_000_000;
    }
    #endregion
}