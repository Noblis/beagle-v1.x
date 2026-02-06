using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq19 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;

        inputs[0] = m;
        inputs[1] = v;
        inputs[2] = c;

        var result = (m * v) / MathF.Sqrt(1f - (v*v) / (c*c));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["m", "v", "c"];
    }
    #endregion
}