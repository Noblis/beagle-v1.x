using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq15 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var z = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = m;
        allInputs[inputSetIdx, 1] = g;
        allInputs[inputSetIdx, 2] = z;

        var result = m * g * z;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "g", "z"];
    }
    #endregion
}