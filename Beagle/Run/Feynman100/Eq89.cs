using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq89 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var e = 1 + Rnd.Random.NextSingle() * 1;   
        var t = 1 + Rnd.Random.NextSingle() * 1;   
        var h = 1 + Rnd.Random.NextSingle() * 3;   

        
        inputs[0] = e;
        inputs[1] = t;
        inputs[2] = h;

        
        var result = MathF.Pow(MathF.Sin((e * t) / h), 2);

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "e", "t", "h" };
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