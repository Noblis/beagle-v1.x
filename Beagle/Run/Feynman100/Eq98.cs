using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq98 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var beta = 1 + Rnd.Random.NextSingle() * 4;   
        var alpha = 1 + Rnd.Random.NextSingle() * 4;  
        var theta = 1 + Rnd.Random.NextSingle() * 4;  

        inputs[0] = beta;
        inputs[1] = alpha;
        inputs[2] = theta;

        
        var result = beta * (1 + alpha * MathF.Cos(theta));

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["beta", "alpha", "Theta"];
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
