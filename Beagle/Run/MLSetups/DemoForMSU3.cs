using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DemoForMSU3 : MLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var g = 1 + Rnd.Random.NextSingle();
        var m1 = 1 + Rnd.Random.NextSingle();
        var m2 = 1 + Rnd.Random.NextSingle();
        var x1 = 3 + Rnd.Random.NextSingle();
        var x2 = 1 + Rnd.Random.NextSingle();
        //var y1 = 3 + Rnd.Random.NextSingle();
        //var y2 = 1 + Rnd.Random.NextSingle();
        //var z1 = 3 + Rnd.Random.NextSingle();
        //var z2 = 1 + Rnd.Random.NextSingle();

        allInputs[inputSetIdx, 0] = g;
        allInputs[inputSetIdx, 1] = m1;
        allInputs[inputSetIdx, 2] = m2;
        allInputs[inputSetIdx, 3] = x1;
        allInputs[inputSetIdx, 4] = x2;
        //inputs[5] = y1;
        //inputs[6] = y2;
        //inputs[7] = z1;
        //inputs[8] = z2;

        var dx = x2 - x1;
        //var dy = y2 - y1;
        //var dz = z2 - z1;

        //var result = (g * m1 * m2)/(dx*dx + dy*dy + dz*dz);
        var result = (g * m1 * m2) / (dx * dx);

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        //return ["G", "m1", "m2", "x1", "x2", "y1", "y2", "z1", "z2"];
        return ["G", "m1", "m2", "x1", "x2"];
    }

    //public override int TargetColonySize(int generation)
    //{
    //    if (generation % 1000 < 25) return 20_000_000;
    //    return 1_000_000;
    //}

    protected override int ScriptLengthTaxRateInternal => BConfig.MaxScore * (int)ExperimentsPerGeneration / 200;

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;

    //public override uint ExperimentsPerGeneration => 1024;

    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;

    //public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Sin &&
    //                                                                                            x != OpEnum.Sqrt &&
    //                                                                                            x != OpEnum.Cbrt &&
    //                                                                                            x != OpEnum.Cube &&
    //                                                                                            x != OpEnum.Ln).ToArray();
    #endregion
}