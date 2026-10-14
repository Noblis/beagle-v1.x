using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq87 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
       
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var omega = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = h;
        allInputs[inputSetIdx, 1] = omega;
        allInputs[inputSetIdx, 2] = k;
        allInputs[inputSetIdx, 3] = t;
     
        var numerator = h * omega;
        var denominator = MathF.Exp(h * omega / (k * t)) - 1f;
        var result = numerator / denominator;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h", "Omega", "k", "T"];
    }
    #endregion
}

