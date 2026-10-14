using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq75 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var v = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = v;
        allInputs[inputSetIdx, 2] = r;

        var result = q * v / (2f * MathF.PI * r);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "v", "r"];
    }
    #endregion
}
