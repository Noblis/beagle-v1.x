using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DemoForMSU4 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = x;

        var result = MathF.Pow(MathF.E, -x*x/2)/MathF.Sqrt(2 * MathF.PI);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x"];
    }

    //public override int TargetColonySize(int generation)
    //{
    //    if (generation % 1000 < 20) return 5_000_000;
    //    return 500_000;
    //}

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    public override uint ExperimentsPerGeneration => 512;

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