using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq33 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var e = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 1 + Rnd.Random.NextSingle() * 1;
	    var e1 = 1 + Rnd.Random.NextSingle() * 1;
	    var r = 1 + Rnd.Random.NextSingle() * 1;
	    var w = 1 + Rnd.Random.NextSingle() * 1;
	    var w1 = 3 + Rnd.Random.NextSingle() * 2;
	    
	    inputs[0] = e;
	    inputs[1] = c;
	    inputs[2] = e1;
	    inputs[3] = r;
	    inputs[4] = w;
	    inputs[5] = w1;
	    
        var result = (1f/2f*e*c*e1*e1) * (8*MathF.PI*r*r/3) * (w*w*w*w/MathF.Pow(w*w-w1*w1, 2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["e","c","E1","r","w","w1"];
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
