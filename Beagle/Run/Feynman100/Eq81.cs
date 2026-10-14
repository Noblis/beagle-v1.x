using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq81 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = n;
        allInputs[inputSetIdx, 1] = u;
        allInputs[inputSetIdx, 2] = b;
        allInputs[inputSetIdx, 3] = k;
        allInputs[inputSetIdx, 4] = t;
    
        var tmp = u * b / (k * t);
        var result = n * u * MathF.Tanh(tmp);

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "u", "B", "k", "T"];
    }
    #endregion
}

