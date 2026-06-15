using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq78 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q;
        inputs[1] = h;
        inputs[2] = m;

        var result = q * h / (4f * MathF.PI * m);

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "h", "m"];
    }
    #endregion
}

