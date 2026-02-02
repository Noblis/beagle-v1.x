using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq38 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var l1 = 1 + Rnd.Random.NextSingle() * 4;
	    var l2 = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = l1;
	    inputs[1] = l2;
	    inputs[2] = t;
	    
        var result = l1+l2+2*MathF.Sqrt(l1*l2)*MathF.Cos(t);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["l1","l2","t"];
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
