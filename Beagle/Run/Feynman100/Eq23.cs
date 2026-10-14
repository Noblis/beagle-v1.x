using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq23 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = m;
        allInputs[inputSetIdx, 1] = r;
        allInputs[inputSetIdx, 2] = v;
        allInputs[inputSetIdx, 3] = theta;

        var result = m * r * v * MathF.Sin(theta);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "r", "v", "Theta"];
    }
    #endregion
}