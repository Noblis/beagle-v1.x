using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq8 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var mu = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = mu;
        allInputs[inputSetIdx, 1] = n;

        var result = mu * n;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Mu", "N"];
    }
    #endregion
}