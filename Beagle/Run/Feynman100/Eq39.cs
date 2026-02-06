using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq39 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var e = 1 + Rnd.Random.NextSingle() * 4;
	    var h = 1 + Rnd.Random.NextSingle() * 4;
	    var m = 1 + Rnd.Random.NextSingle() * 4;
	    var q = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = e;
	    inputs[1] = h;
	    inputs[2] = m;
	    inputs[3] = q;
	    
        var result = (4*MathF.PI*e*h*h)/(m*q*q);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["e","h","m","q"];
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
