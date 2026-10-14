using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq79 : FeynmanMLSetup
{
    #region Overrides

        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var j = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = g;
        allInputs[inputSetIdx, 1] = u;
        allInputs[inputSetIdx, 2] = b;
        allInputs[inputSetIdx, 3] = j;
        allInputs[inputSetIdx, 4] = h;

        var result = g * u * b * j / h;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["g", "u", "B", "J", "h"];
    }
    #endregion
}

