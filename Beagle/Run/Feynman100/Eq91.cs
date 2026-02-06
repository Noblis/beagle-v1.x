using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq91 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var u  = 1 + Rnd.Random.NextSingle() * 4;   
        var Bx = 1 + Rnd.Random.NextSingle() * 4;   
        var By = 1 + Rnd.Random.NextSingle() * 4;   
        var Bz = 1 + Rnd.Random.NextSingle() * 4;   

        
        inputs[0] = u;
        inputs[1] = Bx;
        inputs[2] = By;
        inputs[3] = Bz;

        
        var result = u * MathF.Sqrt(Bx * Bx + By * By + Bz * Bz);

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "u", "Bx", "By", "Bz" };
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