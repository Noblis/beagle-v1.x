using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class EQ48 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    var v2 = 1 + Rnd.Random.NextSingle() * 4;
        var v1 = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = n;
	    inputs[1] = k;
        inputs[2] = t;
        inputs[3] = v2;
        inputs[4] = v1;
        
        var result = n*k*t*MathF.Log(v2/v1);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","k","t","v2","v1"];
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
