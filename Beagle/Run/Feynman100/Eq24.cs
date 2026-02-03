using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq24 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 2;
        var w = 1 + Rnd.Random.NextSingle() * 2;
        var w1 = 1 + Rnd.Random.NextSingle() * 2;
        var x = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = m;
        inputs[1] = w;
        inputs[2] = w1;
        inputs[3] = x;

        var result = 0.25f * m * (w*w + w1*w1) * (x*x);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "w", "w1", "x"];
    }
    #endregion
}