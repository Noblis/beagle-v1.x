using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq60 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var sigma = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var chi = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = sigma;
        allInputs[inputSetIdx, 1] = eps;
        allInputs[inputSetIdx, 2] = chi;

        var result = (sigma / eps) * (1f / (1f + chi));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Sigma", "Eps", "Chi"];
    }
    #endregion
}
