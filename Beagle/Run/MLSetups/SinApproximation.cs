using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class SinApproximation : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x = Rnd.Random.NextSingle() * 2 * (float)Math.PI - (float)Math.PI;
        inputs[0] = x;
        var output = MathF.Sin(x);
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["x"];
    }
    public override int TargetColonySize(int generation)
    {
        if (generation % 1000 < 20) return 5_000_000;
        return 1_000_000;

        //if (generation < 10) return 1_000_000_000;
        //if (generation < 20) return 250_000_000;
        //if (generation < 50) return 100_000_000;
        //if (generation < 100) return 10_000_000;

        //if (generation % 1000 < 20) return 20_000_000;
        //if (generation % 1000 < 100) return 3_000_000;
        //return 1_000_000;
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 20_000_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override double SolutionFoundASRThreshold => 1.0; //0.9999;
    public override uint ExperimentsPerGeneration => 1024;

    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Sin && x != OpEnum.Cos).ToArray();
    #endregion
}