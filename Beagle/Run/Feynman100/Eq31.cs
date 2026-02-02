using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq31 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var l = 1 + Rnd.Random.NextSingle() * 1;
	    var n = 1 + Rnd.Random.NextSingle() * 4;
	    var d = 2 + Rnd.Random.NextSingle() * 3;
	    
	    inputs[0] = l;
	    inputs[1] = n;
	    inputs[2] = d;
	    
        var result = MathF.Asin(l/(n*d));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["L","n","t"];
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
