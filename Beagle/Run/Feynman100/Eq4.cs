using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class FeynmanEq4 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var x3 = 1 + Rnd.Random.NextSingle() * 4;
        var x4 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;
        inputs[3] = x4;

        var result = MathF.Sqrt((x2 - x1)*(x2 - x1) + (x4 - x3)*(x4 - x3));
        //var result = MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(x4 - x3, 2));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "x3", "x4"];
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube).ToArray();
    #endregion
}
