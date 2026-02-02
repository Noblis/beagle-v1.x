using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class FeynmanEq7 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var x3 = 1 + Rnd.Random.NextSingle() * 4;
        var x4 = 1 + Rnd.Random.NextSingle() * 4;
        var x5 = 1 + Rnd.Random.NextSingle() * 4;
        var x6 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;
        inputs[3] = x4;
        inputs[4] = x5;
        inputs[5] = x6;

        var result = x1*x4 + x2*x5 + x3*x6;
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "x3", "x4", "x5", "x6"];
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube).ToArray();
    #endregion
}