using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

/// <summary>Fixed-colony-size quick benchmark harness (stable workload for before/after comparisons).</summary>
public class QuickBenchmark : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x = Rnd.Random.NextSingle() * 2 * (float)Math.PI - (float)Math.PI;
        inputs[0] = x;
        var output = Rnd.Random.NextSingle() * 2 * (float)Math.PI - (float)Math.PI;
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["x"];
    }

    // Fixed colony size: stable births/deaths per generation, no colony-reset schedule.
    public override int TargetColonySize(int generation) => 1_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override uint ExperimentsPerGeneration => 512;
    public static uint Desired { get; set; }
    public override uint DesiredGroupSize => Desired;
    public override long TotalBirthsToResetColonyIfNoProgress => long.MaxValue / 4;
    #endregion
}
