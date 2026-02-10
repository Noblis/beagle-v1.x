using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq99 : FeynmanMLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var eps = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = m;
        inputs[1] = q;
        inputs[2] = eps;
        inputs[3] = h;
        inputs[4] = n;
        
        var term = 4f * MathF.PI * eps;
        var denominator = 2f * term*term * h*h;
        var result = -m * q*q*q*q / denominator * (1f / (n*n));

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "q", "Eps", "h", "n"];
    }
    #endregion
}
