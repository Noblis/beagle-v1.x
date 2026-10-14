using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq99 : FeynmanMLSetup
{
    #region Overrides

        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var eps = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = m;
        allInputs[inputSetIdx, 1] = q;
        allInputs[inputSetIdx, 2] = eps;
        allInputs[inputSetIdx, 3] = h;
        allInputs[inputSetIdx, 4] = n;
        
        var term = 4f * MathF.PI * eps;
        var denominator = 2f * term*term * h*h;
        var result = -m * q*q*q*q / denominator * (1f / (n*n));

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "q", "Eps", "h", "n"];
    }
    #endregion
}
