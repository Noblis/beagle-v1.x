using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq86 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var omega = 1 + Rnd.Random.NextSingle() * 2;  
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = h;
        inputs[1] = omega;
        inputs[2] = k;
        inputs[3] = t;

        var result = 1f / (MathF.Exp(h * omega / (k * t)) - 1f);

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h", "Omega", "k", "T"];
    }
    #endregion
}

