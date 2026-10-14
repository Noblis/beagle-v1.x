using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq88 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
        allInputs[inputSetIdx, 1] = b;
        allInputs[inputSetIdx, 2] = h;
       
        var result = 2f * u * b / h;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "B", "h"];
    }
    #endregion
}

