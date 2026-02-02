using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq49 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var y = 1 + Rnd.Random.NextSingle() * 4;
	    var pr = 1 + Rnd.Random.NextSingle() * 4;
        var p = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = y;
	    inputs[1] = pr;
	    inputs[2] = p;

        var result = MathF.Sqrt(y*pr*p);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["y","pr","p"];
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
