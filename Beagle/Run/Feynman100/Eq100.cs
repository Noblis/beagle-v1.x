using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq100 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var p = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var A = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = p;
        inputs[1] = q;
        inputs[2] = A;
        inputs[3] = m;

        
        var result = (-p * q * A) / m;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "p", "q", "A", "m" };
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
