using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq59 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var e = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = eps;
        allInputs[inputSetIdx, 1] = e;

        var result = eps * e*e / 2;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "E"];
    }
    #endregion
}
