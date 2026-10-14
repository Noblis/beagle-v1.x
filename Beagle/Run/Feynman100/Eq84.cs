using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq84 : FeynmanMLSetup
{
    #region Overrides

        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var y = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = y;
        allInputs[inputSetIdx, 1] = a;
        allInputs[inputSetIdx, 2] = x;
        allInputs[inputSetIdx, 3] = d;

        var result = y * a * x / d;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Y", "A", "x", "d"];
    }
    #endregion
}

