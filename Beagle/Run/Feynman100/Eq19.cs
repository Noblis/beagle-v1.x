using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq19 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;

        allInputs[inputSetIdx, 0] = m;
        allInputs[inputSetIdx, 1] = v;
        allInputs[inputSetIdx, 2] = c;

        var result = (m * v) / MathF.Sqrt(1f - (v*v) / (c*c));
        return (allInputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["m", "v", "c"];
    }
    #endregion
}