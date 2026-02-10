using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq81 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = n;
        inputs[1] = u;
        inputs[2] = b;
        inputs[3] = k;
        inputs[4] = t;
    
        var tmp = u * b / (k * t);
        var result = n * u * MathF.Tanh(tmp);

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "u", "B", "k", "T"];
    }
    #endregion
}

