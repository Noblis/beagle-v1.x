using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq100 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var p = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = p;
        allInputs[inputSetIdx, 1] = q;
        allInputs[inputSetIdx, 2] = a;
        allInputs[inputSetIdx, 3] = m;

        var result = -p * q * a / m;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "q", "A", "m"];
    }
    #endregion
}
