using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq82 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 2;
        var b = 1 + Rnd.Random.NextSingle() * 2;
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;
        var alpha = 1 + Rnd.Random.NextSingle() * 2;
        var m = 1 + Rnd.Random.NextSingle() * 2;
        var eps = 1 + Rnd.Random.NextSingle() * 2;
        var c = 1 + Rnd.Random.NextSingle() * 2;

        allInputs[inputSetIdx, 0] = u;
        allInputs[inputSetIdx, 1] = b;
        allInputs[inputSetIdx, 2] = k;
        allInputs[inputSetIdx, 3] = t;
        allInputs[inputSetIdx, 4] = alpha;
        allInputs[inputSetIdx, 5] = m;
        allInputs[inputSetIdx, 6] = eps;
        allInputs[inputSetIdx, 7] = c;

        var term1 = u * b / (k * t);
        var term2 = u * alpha * m / (eps * c * c * k * t);
        var result = term1 + term2;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "B", "k", "T", "Alpha", "M", "Eps", "c"];
    }
    #endregion
}

