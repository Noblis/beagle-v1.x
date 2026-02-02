using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq86 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var omega = 1 + Rnd.Random.NextSingle() * 2;  
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var T = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = h;
        inputs[1] = omega;
        inputs[2] = k;
        inputs[3] = T;

        var result = 1f / (MathF.Exp((h * omega) / (k * T)) - 1f);

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "h", "omega", "k", "T" };
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

