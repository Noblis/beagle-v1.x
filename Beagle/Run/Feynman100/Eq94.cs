using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq94 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var l = 1 + Rnd.Random.NextSingle() * 4;   
        var q = 1 + Rnd.Random.NextSingle() * 1;   
        var v = 1 + Rnd.Random.NextSingle() * 1;   
        var k = 1 + Rnd.Random.NextSingle() * 1;   
        var t = 1 + Rnd.Random.NextSingle() * 1;

        allInputs[inputSetIdx, 0] = l;
        allInputs[inputSetIdx, 1] = q;
        allInputs[inputSetIdx, 2] = v;
        allInputs[inputSetIdx, 3] = k;
        allInputs[inputSetIdx, 4] = t;
        
        var result = l * (MathF.Exp(q * v / (k * t)) - 1f);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["l", "q", "V", "k", "T"];
    }
    #endregion
}