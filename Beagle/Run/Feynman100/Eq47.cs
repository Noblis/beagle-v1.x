using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class EQ47 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var y = 2 + Rnd.Random.NextSingle() * 3;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = y;
	    inputs[1] = k;
        inputs[2] = v;
        inputs[3] = a;

        var result = 1/(y-1) * k*v/a;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["y","k","v","a"];
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
