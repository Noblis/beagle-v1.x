using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq84 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var Y = 1 + Rnd.Random.NextSingle() * 4;
        var A = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = Y;
        inputs[1] = A;
        inputs[2] = x;
        inputs[3] = d;

        var result = (Y * A * x) / d;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "Y", "A", "x", "d" };
    }


    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;


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

