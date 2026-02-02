using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class EQ45 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
	    var q = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
	    var d = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = u;
	    inputs[1] = q;
	    inputs[2] = v;
	    inputs[3] = d;

        var result = u*q*v/d;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u","q","v","d"];
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
