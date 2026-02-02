using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class EQ41 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var y = 2 + Rnd.Random.NextSingle() * 3;
        var p = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = y;
	    inputs[1] = p;
	    inputs[2] = v;
	    
        var result = 1/(y-1)*p*v;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["y","p","v"];
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
