using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class QuadraticEqNormalized : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var b = Rnd.Random.NextSingle() * 200 - 100;
        var c = Rnd.Random.NextSingle() * 200 - 100;
        inputs[0] = b;
        inputs[1] = c;
        var output = (MathF.Sqrt(b * b - 4 * c) - b) / 2;
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["b", "c"];
    }

    public override int TargetColonySize(int generation)
    {
        if (generation % 1000 < 15) return 10_000_000;
        return 1_000_000;
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 300_000_000;
    #endregion
}