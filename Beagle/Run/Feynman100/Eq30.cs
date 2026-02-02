using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class EQ30 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var l = 1 + Rnd.Random.NextSingle() * 4;
	    var n = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = l;
	    inputs[1] = n;
	    inputs[2] = t;
	    
        var result = l*MathF.Pow(MathF.Sin(n*t/2),2)/MathF.Pow(MathF.Sin(t/2),2);
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
