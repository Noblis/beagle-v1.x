using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq17 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x = 5 + Rnd.Random.NextSingle() * 5;
        var u = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 17;
        var t = 1 + Rnd.Random.NextSingle() * 1;

        allInputs[inputSetIdx, 0] = x;
        allInputs[inputSetIdx, 1] = u;
        allInputs[inputSetIdx, 2] = c;
        allInputs[inputSetIdx, 3] = t;

        var result = (x - u*t) / MathF.Sqrt(1f - (u*u) / (c*c));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x", "u", "c", "t"];
    }
    #endregion
}