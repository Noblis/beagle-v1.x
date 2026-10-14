using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq86 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var omega = 1 + Rnd.Random.NextSingle() * 2;  
        var k = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;

        allInputs[inputSetIdx, 0] = h;
        allInputs[inputSetIdx, 1] = omega;
        allInputs[inputSetIdx, 2] = k;
        allInputs[inputSetIdx, 3] = t;

        var result = 1f / (MathF.Exp(h * omega / (k * t)) - 1f);

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h", "Omega", "k", "T"];
    }
    #endregion
}

