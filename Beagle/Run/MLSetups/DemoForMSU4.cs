using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class DemoForMSU4 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var o = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = o;

        var result = MathF.Pow(MathF.E, -o*o/2)/MathF.Sqrt(2 * MathF.PI);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["O"];
    }

    public override int TargetColonySize(int generation)
    {
        if (generation % 1000 < 20) return 50_000_000;
        return 1_000_000;
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override uint ExperimentsPerGeneration => 1024;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;

    //public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Sin &&
    //                                                                                          x != OpEnum.Add &&
    //                                                                                          x != OpEnum.Sub &&
    //                                                                                          x != OpEnum.Cbrt &&
    //                                                                                          x != OpEnum.Cube &&
    //                                                                                          x != OpEnum.Ln).ToArray();

    #endregion
}