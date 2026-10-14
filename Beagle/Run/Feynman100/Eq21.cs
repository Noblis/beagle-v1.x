using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq21 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m1 = 1 + Rnd.Random.NextSingle() * 4;
        var m2 = 1 + Rnd.Random.NextSingle() * 4;
        var r1 = 1 + Rnd.Random.NextSingle() * 4;
        var r2 = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = m1;
        allInputs[inputSetIdx, 1] = m2;
        allInputs[inputSetIdx, 2] = r1;
        allInputs[inputSetIdx, 3] = r2;

        var result = (m1*r1 + m2*r2) / (m1 + m2);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m1", "m2", "r1", "r2"];
    }
    #endregion
}