using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq82 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {

        var u = 1 + Rnd.Random.NextSingle() * 2;
        var B = 1 + Rnd.Random.NextSingle() * 2;
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var T = 1 + Rnd.Random.NextSingle() * 2;
        var alpha = 1 + Rnd.Random.NextSingle() * 2;
        var M = 1 + Rnd.Random.NextSingle() * 2;
        var eps = 1 + Rnd.Random.NextSingle() * 2;
        var c = 1 + Rnd.Random.NextSingle() * 2;

    
        inputs[0] = u;
        inputs[1] = B;
        inputs[2] = k;
        inputs[3] = T;
        inputs[4] = alpha;
        inputs[5] = M;
        inputs[6] = eps;
        inputs[7] = c;

        var term1 = (u * B) / (k * T);
        var term2 = (u * alpha * M) / (eps * c * c * k * T);
        var result = term1 + term2;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "u", "B", "k", "T", "alpha", "M", "eps", "c" };
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

