using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq37 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = h;
	    inputs[1] = w;
	    
        var result = h*w;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h","w"];
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
