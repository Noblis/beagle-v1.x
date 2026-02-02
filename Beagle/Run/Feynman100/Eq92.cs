using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq92 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var n = 1 + Rnd.Random.NextSingle() * 4;   
        var h = 1 + Rnd.Random.NextSingle() * 4;   

        
        inputs[0] = n;
        inputs[1] = h;

        
        var result = n * h;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "n", "h" };
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