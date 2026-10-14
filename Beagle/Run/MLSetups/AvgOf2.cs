using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class AvgOf2 : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var a = Rnd.Random.NextSingle() * 100;
        var b = Rnd.Random.NextSingle() * 100;

        allInputs[inputSetIdx, 0] = a;
        allInputs[inputSetIdx, 1] = b;
        
        return (allInputs, (a + b) / 2);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b"];
    }

    public override int TargetColonySize(int generation)
    {
        if (generation == 0) return 500;
        
        return 1_000_000;
    }

    public override double SolutionFoundASRThreshold => 1.0;
    #endregion
}