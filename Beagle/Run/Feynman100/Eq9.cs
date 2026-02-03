using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq9 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var w = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = m;
        inputs[1] = v;
        inputs[2] = u;
        inputs[3] = w;

        var result = 0.5f * m * (v*v + u*u + w*w);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "v", "u", "w"];
    }
    #endregion
}