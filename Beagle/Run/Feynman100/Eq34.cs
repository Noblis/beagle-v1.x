using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq34 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
	    var b = 1 + Rnd.Random.NextSingle() * 4;
	    var p = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = q;
	    inputs[1] = v;
	    inputs[2] = b;
	    inputs[3] = p;
	    
        var result = q*v*b/p;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q","v","B","p"];
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
