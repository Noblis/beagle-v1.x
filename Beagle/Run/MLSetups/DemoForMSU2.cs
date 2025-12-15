using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class DemoForMSU2 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle();
        var c = 3 + Rnd.Random.NextSingle() * 7;

        inputs[0] = x;
        inputs[1] = v;
        inputs[2] = c;

        var result = x / MathF.Sqrt(1 - (v*v)/(c*c));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x", "v", "c"];
    }

    //public override int TargetColonySize(int generation)
    //{
    //    if (generation % 1000 < 25) return 15_000_000;
    //    return 1_000_000;
    //}

    public override long TotalBirthsToResetColonyIfNoProgress => 500_000_000;

    //public override uint ExperimentsPerGeneration => 1024;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;

    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Sin &&
                                                                                              x != OpEnum.Cbrt &&
                                                                                              x != OpEnum.Cube &&
                                                                                              x != OpEnum.Ln &&
                                                                                              x != OpEnum.Pow).ToArray();
    #endregion
}