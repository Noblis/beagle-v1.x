using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq28 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var w = 1 + Rnd.Random.NextSingle() * 9;
	    var c = 1 + Rnd.Random.NextSingle() * 9;
        
	    inputs[0] = w;
	    inputs[1] = c;

        var result = w/c;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["w","c"];
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
