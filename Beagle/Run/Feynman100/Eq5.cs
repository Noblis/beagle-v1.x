using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq5 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var g = 1 + Rnd.Random.NextSingle() * 1;
        var m1 = 1 + Rnd.Random.NextSingle() * 1;
        var m2 = 1 + Rnd.Random.NextSingle() * 1;
        var x1 = 3 + Rnd.Random.NextSingle() * 1;
        var x2 = 1 + Rnd.Random.NextSingle() * 1;
        var y1 = 3 + Rnd.Random.NextSingle() * 1;
        var y2 = 1 + Rnd.Random.NextSingle() * 1;
        var z1 = 3 + Rnd.Random.NextSingle() * 1;
        var z2 = 1 + Rnd.Random.NextSingle() * 1;

        inputs[0] = g;
        inputs[1] = m1;
        inputs[2] = m2;
        inputs[3] = x1;
        inputs[4] = x2;
        inputs[5] = y1;
        inputs[6] = y2;
        inputs[7] = z1;
        inputs[8] = z2;

        var dx = x2 - x1;
        var dy = y2 - y1;
        var dz = z2 - z1;
        var result = (g * m1 * m2) / (dx*dx + dy*dy + dz*dz);
        //var result = (g * m1 * m2) / (MathF.Pow(x2 - x1, 2) + MathF.Pow(y2 - y1, 2) + MathF.Pow(z2 - z1, 2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["G", "m1", "m2", "x1", "x2", "y1", "y2", "z1", "z2"];
    }
    #endregion
}