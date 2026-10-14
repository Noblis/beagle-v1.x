using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq97 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var alpha = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = alpha;
        allInputs[inputSetIdx, 1] = n;
        allInputs[inputSetIdx, 2] = d;

        var result = 2f * MathF.PI * alpha / (n * d);

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Alpha", "n", "d"];
    }
    #endregion
}
