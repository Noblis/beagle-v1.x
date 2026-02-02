using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq29 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
	    var x2 = 1 + Rnd.Random.NextSingle() * 4;
	    var t1 = 1 + Rnd.Random.NextSingle() * 4;
	    var t2 = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = x1;
	    inputs[1] = x2;
	    inputs[2] = t1;
	    inputs[3] = t2;

        var result = MathF.Sqrt(x1*x1 + x2*x2 - 2 * x1 * x2 * MathF.Cos(t1-t2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1","x2","t1","t2"];
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
