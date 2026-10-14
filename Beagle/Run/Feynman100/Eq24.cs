using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq24 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 2;
        var w = 1 + Rnd.Random.NextSingle() * 2;
        var w1 = 1 + Rnd.Random.NextSingle() * 2;
        var x = 1 + Rnd.Random.NextSingle() * 2;

        allInputs[inputSetIdx, 0] = m;
        allInputs[inputSetIdx, 1] = w;
        allInputs[inputSetIdx, 2] = w1;
        allInputs[inputSetIdx, 3] = x;

        var result = 0.25f * m * (w*w + w1*w1) * (x*x);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "w", "w1", "x"];
    }
    #endregion
}