using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq80 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var n = 1 + Rnd.Random.NextSingle() * 2;
        var u = 1 + Rnd.Random.NextSingle() * 2;
        var B = 1 + Rnd.Random.NextSingle() * 2;
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var T = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = n;
        inputs[1] = u;
        inputs[2] = B;
        inputs[3] = k;
        inputs[4] = T;

    
        var denominator = MathF.Exp((u * B) / (k * T)) + MathF.Exp(-(u * B) / (k * T));
        var result = n / denominator;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "n", "u", "B", "k", "T" };
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

