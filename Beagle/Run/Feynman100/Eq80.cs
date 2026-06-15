using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq80 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 2;
        var u = 1 + Rnd.Random.NextSingle() * 2;
        var b = 1 + Rnd.Random.NextSingle() * 2;
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = n;
        inputs[1] = u;
        inputs[2] = b;
        inputs[3] = k;
        inputs[4] = t;
    
        var denominator = MathF.Exp(u * b / (k * t)) + MathF.Exp(-u * b / (k * t));
        var result = n / denominator;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "u", "B", "k", "T"];
    }
    #endregion
}

