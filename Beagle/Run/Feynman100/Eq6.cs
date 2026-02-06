using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq6 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m0 = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;

        inputs[0] = m0;
        inputs[1] = v;
        inputs[2] = c;

        var result = m0 / MathF.Sqrt(1f - (v*v) / (c*c));
        //var result = x1 / MathF.Sqrt(1f - MathF.Pow(x2, 2) / MathF.Pow(x3, 2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m0", "v", "c"];
    }
    #endregion
}