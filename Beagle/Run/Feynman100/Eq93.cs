using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq93 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var e = 1 + Rnd.Random.NextSingle() * 4;   
        var d = 1 + Rnd.Random.NextSingle() * 4;   
        var k = 1 + Rnd.Random.NextSingle() * 4;   
        var h = 1 + Rnd.Random.NextSingle() * 4;   

        
        inputs[0] = e;
        inputs[1] = d;
        inputs[2] = k;
        inputs[3] = h;

        
        var result = (2f * e * MathF.Pow(d, 2) * k) / h;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "E", "d", "k", "h" };
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
