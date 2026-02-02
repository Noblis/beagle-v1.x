using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class EQ27 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var d1 = 1 + Rnd.Random.NextSingle() * 4;
        var d2 = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = n;
	    inputs[1] = d1;
	    inputs[2] = d2;

        var result = 1/((1/d1)+(n/d2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","d1","d2"];
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
