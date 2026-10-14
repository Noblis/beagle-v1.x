using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq25 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var c = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = c;

        var result = q / c;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "c"];
    }
    #endregion
}