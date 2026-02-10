using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq99 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var eps = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = m;
        inputs[1] = q;
        inputs[2] = eps;
        inputs[3] = h;
        inputs[4] = n;

        
        var numerator = -m * MathF.Pow(q, 4);
        var denominator = 2 * MathF.Pow(4 * MathF.PI * eps, 2) * MathF.Pow(h, 2);
        var result = numerator / denominator * (1 / MathF.Pow(n, 2));

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["m", "q", "epsilon", "h", "n"];
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
