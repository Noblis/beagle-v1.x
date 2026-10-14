using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq83 : FeynmanMLSetup
{
    #region Overrides

        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
        allInputs[inputSetIdx, 1] = x;
        allInputs[inputSetIdx, 2] = b;

        var result = u * (1f + x) * b;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "x", "B"];
    }
    #endregion
}

