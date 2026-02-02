using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class EQ50 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        
	    inputs[0] = m;
	    inputs[1] = c;
        inputs[2] = v;

        var result = m*c*c/(MathF.Sqrt(1-v*v/(c*c)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m","c","v"];
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
