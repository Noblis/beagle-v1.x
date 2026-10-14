using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq22 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var r = 1 + Rnd.Random.NextSingle() * 4;
        var f = 1 + Rnd.Random.NextSingle() * 4;
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = r;
        allInputs[inputSetIdx, 1] = f;
        allInputs[inputSetIdx, 2] = theta;

        var result = r * f * MathF.Sin(theta);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["r", "f", "Theta"];
    }
    #endregion
}