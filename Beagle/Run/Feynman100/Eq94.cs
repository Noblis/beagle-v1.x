using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq94 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var l = 1 + Rnd.Random.NextSingle() * 4;   
        var q = 1 + Rnd.Random.NextSingle() * 1;   
        var V = 1 + Rnd.Random.NextSingle() * 1;   
        var k = 1 + Rnd.Random.NextSingle() * 1;   
        var T = 1 + Rnd.Random.NextSingle() * 1;   

        
        inputs[0] = l;
        inputs[1] = q;
        inputs[2] = V;
        inputs[3] = k;
        inputs[4] = T;

        
        var result = l * (MathF.Exp((q * V) / (k * T)) - 1f);

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "l", "q", "V", "k", "T" };
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