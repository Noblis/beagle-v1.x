using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq58 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var d = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = eps;
        allInputs[inputSetIdx, 2] = d;

        var result = (3f / 5f) * (q * q / (4f * MathF.PI * eps * d));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Eps", "d"];
    }
    #endregion
}
