using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq12 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q2 = 1 + Rnd.Random.NextSingle() * 4;
        var e = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = q2;
        allInputs[inputSetIdx, 1] = e;

        var result = q2 * e;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q2", "E"];
    }
    #endregion
}