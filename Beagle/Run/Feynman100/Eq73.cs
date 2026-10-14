using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq73 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var c = Rnd.Random.NextSingle() * 4 + 1;
        var e = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = eps;
        allInputs[inputSetIdx, 1] = c;
        allInputs[inputSetIdx, 2] = e;

        // f = eps * c * E^2
        var result = eps * c * e*e;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "c", "E"];
    }
    #endregion
}
