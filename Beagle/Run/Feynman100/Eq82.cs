using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq82 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 2;
        var b = 1 + Rnd.Random.NextSingle() * 2;
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;
        var alpha = 1 + Rnd.Random.NextSingle() * 2;
        var m = 1 + Rnd.Random.NextSingle() * 2;
        var eps = 1 + Rnd.Random.NextSingle() * 2;
        var c = 1 + Rnd.Random.NextSingle() * 2;
    
        inputs[0] = u;
        inputs[1] = b;
        inputs[2] = k;
        inputs[3] = t;
        inputs[4] = alpha;
        inputs[5] = m;
        inputs[6] = eps;
        inputs[7] = c;

        var term1 = u * b / (k * t);
        var term2 = u * alpha * m / (eps * c * c * k * t);
        var result = term1 + term2;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "B", "k", "T", "Alpha", "M", "Eps", "c"];
    }
    #endregion
}

