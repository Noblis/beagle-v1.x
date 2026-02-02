using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq95 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var U = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        
        inputs[0] = U;
        inputs[1] = k;
        inputs[2] = d;

        
        var result = 2f * U * (1f - MathF.Cos(k * d));

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "U", "k", "d" };
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
