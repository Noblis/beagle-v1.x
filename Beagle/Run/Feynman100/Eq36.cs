using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq36 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var v = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = v;
	    inputs[1] = c;
	    inputs[2] = w;
	    
        var result = (1+v/c)/(MathF.Sqrt(1-v*v/(c*c)))*w;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["v","c","w"];
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
