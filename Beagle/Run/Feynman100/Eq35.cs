using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class EQ35 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var w = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
	    
	    inputs[0] = w;
	    inputs[1] = v;
	    inputs[2] = c;
	    
        var result = w/(1-v/c);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["w","v","c"];
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
