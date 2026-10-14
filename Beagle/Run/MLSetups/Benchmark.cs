using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class Benchmark : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x = Rnd.Random.NextSingle() * 2 * (float)Math.PI - (float)Math.PI;

        allInputs[inputSetIdx, 0] = x;
        
        var output = Rnd.Random.NextSingle() * 2 * (float)Math.PI - (float)Math.PI;

        return (allInputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["x"];
    }
    public override int TargetColonySize(int generation)
    {
        return 30_000_000;
    }
    public override double SolutionFoundASRThreshold => 1.0;
    public override uint ExperimentsPerGeneration => 1024;
    #endregion
}