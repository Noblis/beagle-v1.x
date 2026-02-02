using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class Eq3 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 2;
        var x2 = 1 + Rnd.Random.NextSingle() * 2;
        var x3 = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;
        // Checked

        var tmp = (x1 - x2) / x3;
        var result = MathF.Exp(-tmp*tmp / 2f) / (MathF.Sqrt(2f * MathF.PI) * x3);
        //var result = MathF.Exp(-MathF.Pow((x1 - x2) / x3, 2) / 2f) / (MathF.Sqrt(2f * MathF.PI) * x3);
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "x3"];
    }

    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Cbrt && x != OpEnum.Cube && x != OpEnum.Pow).ToArray();
    #endregion
}
