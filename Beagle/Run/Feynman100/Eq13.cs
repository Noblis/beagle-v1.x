using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq13 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var ef = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = ef;
        allInputs[inputSetIdx, 2] = b;
        allInputs[inputSetIdx, 3] = v;
        allInputs[inputSetIdx, 4] = theta;

        var result = q * (ef + b * v * MathF.Sin(theta));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Ef", "B", "v", "Theta"];
    }
    #endregion
}