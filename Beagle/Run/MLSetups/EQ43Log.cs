using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class Eq43Log : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var w = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = n;
        inputs[1] = m;
        inputs[2] = w;
        inputs[3] = x;
        inputs[4] = k;
        inputs[5] = t;

        var result = MathF.Log(n * MathF.Exp(m * w * x / (k * t)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "m", "w", "x", "k", "t"];
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    #endregion
}