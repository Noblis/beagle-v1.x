using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq76 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
    
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q;
        inputs[1] = v;
        inputs[2] = r;

    
        var result = q * v * r / 2.0f;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "q", "v", "r" };
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