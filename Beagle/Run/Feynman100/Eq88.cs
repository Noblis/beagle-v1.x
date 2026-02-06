using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq88 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {

        var u = 1 + Rnd.Random.NextSingle() * 4;
        var B = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;

        
        inputs[0] = u;
        inputs[1] = B;
        inputs[2] = h;

       
        var result = (2 * u * B) / h;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "u", "B", "h" };
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

