using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class Eq79 : MLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var B = 1 + Rnd.Random.NextSingle() * 4;
        var J = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;

       
        inputs[0] = g;
        inputs[1] = u;
        inputs[2] = B;
        inputs[3] = J;
        inputs[4] = h;

        var result = g * u * B * J / h;

        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return new[] { "g", "u", "B", "J", "h" };
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

