using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq80 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 1 + Rnd.Random.NextSingle() * 2;
        var u = 1 + Rnd.Random.NextSingle() * 2;
        var b = 1 + Rnd.Random.NextSingle() * 2;
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;

        allInputs[inputSetIdx, 0] = n;
        allInputs[inputSetIdx, 1] = u;
        allInputs[inputSetIdx, 2] = b;
        allInputs[inputSetIdx, 3] = k;
        allInputs[inputSetIdx, 4] = t;
    
        var denominator = MathF.Exp(u * b / (k * t)) + MathF.Exp(-u * b / (k * t));
        var result = n / denominator;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "u", "B", "k", "T"];
    }
    #endregion
}

