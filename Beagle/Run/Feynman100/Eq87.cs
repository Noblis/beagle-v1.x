using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq87 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
       
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var omega = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var T = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = h;
        inputs[1] = omega;
        inputs[2] = k;
        inputs[3] = T;

     
        var numerator = h * omega;
        var denominator = MathF.Exp((h * omega) / (k * T)) - 1f;
        var result = numerator / denominator;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "h", "omega", "k", "T" };
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

