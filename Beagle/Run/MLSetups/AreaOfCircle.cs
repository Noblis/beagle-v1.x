using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class AreaOfCircle : MLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var r = Rnd.Random.NextSingle() * 25;

        allInputs[inputSetIdx, 0] = r;
        
        return (allInputs, r*r*MathF.PI);
    }
    public override string[] GetInputLabels()
    {
        return ["r"];
    }

    public override uint ExperimentsPerGeneration => 512;

    public override long TotalBirthsToResetColonyIfNoProgress => 2_000_000;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}