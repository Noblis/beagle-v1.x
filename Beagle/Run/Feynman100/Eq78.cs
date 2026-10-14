using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq78 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = h;
        allInputs[inputSetIdx, 2] = m;

        var result = q * h / (4f * MathF.PI * m);

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "h", "m"];
    }
    #endregion
}

