using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq96 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var e = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        
        inputs[0] = h;
        inputs[1] = e;
        inputs[2] = d;

        
        var result = (h * h) / (2f * e * d * d);

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "h", "e", "d" };
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