using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq85 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var y = 1 + Rnd.Random.NextSingle() * 4;
        var sigma = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = y;
        allInputs[inputSetIdx, 1] = sigma;

        var result = y / (2f * (1f + sigma));

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Y", "sigma"];
    }
    #endregion
}

