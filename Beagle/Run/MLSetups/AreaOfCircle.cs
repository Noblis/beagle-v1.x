using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class AreaOfCircle : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var r = Rnd.Random.NextSingle() * 1000;
        inputs[0] = r;
        return (inputs, r*r*MathF.PI);
    }
    public override string[] GetInputLabels()
    {
        return ["r"];
    }

    public override uint ExperimentsPerGeneration => 1024;

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