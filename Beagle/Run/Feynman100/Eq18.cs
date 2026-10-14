using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq18 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var t = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;
        var x = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = t;
        allInputs[inputSetIdx, 1] = u;
        allInputs[inputSetIdx, 2] = c;
        allInputs[inputSetIdx, 3] = x;

        var result = (t - u*x / (c*c)) / MathF.Sqrt(1f - (u*u) / (c*c));

        return (allInputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["t", "u", "c", "x"];
    }
    #endregion
}