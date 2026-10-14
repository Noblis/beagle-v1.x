using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq74 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // ranges: {1,5} for eps and E
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var e = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = eps;
        allInputs[inputSetIdx, 1] = e;

        var result = eps * e * e;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "E"];
    }
    #endregion
}
