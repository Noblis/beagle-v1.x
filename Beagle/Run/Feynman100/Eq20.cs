using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq20 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var c = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
        allInputs[inputSetIdx, 1] = v;
        allInputs[inputSetIdx, 2] = c;

        var result = (u + v) / (1f + (u * v / (c*c)));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "v", "c"];
    }
    #endregion
}