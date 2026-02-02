using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class EQ43 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var m = 1 + Rnd.Random.NextSingle() * 4;
	    var g = 1 + Rnd.Random.NextSingle() * 4;
	    var x = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = n;
	    inputs[1] = m;
	    inputs[2] = g;
	    inputs[3] = x;
	    inputs[4] = k;
	    inputs[5] = t;

        var result = n*MathF.Exp(m*g*x/(k*t));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","m","g","x","k","t"];
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
