using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq32 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
	    var a = 1 + Rnd.Random.NextSingle() * 4;
	    var e = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = q;
	    inputs[1] = a;
	    inputs[2] = e;
	    inputs[3] = c;
	    
        var result = q*q*a*a/(6*MathF.PI*e*c*c*c);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q","a","e","c"];
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
