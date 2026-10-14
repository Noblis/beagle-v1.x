using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq7 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var x3 = 1 + Rnd.Random.NextSingle() * 4;
        var y1 = 1 + Rnd.Random.NextSingle() * 4;
        var y2 = 1 + Rnd.Random.NextSingle() * 4;
        var y3 = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = x1;
        allInputs[inputSetIdx, 1] = x2;
        allInputs[inputSetIdx, 2] = x3;
        allInputs[inputSetIdx, 3] = y1;
        allInputs[inputSetIdx, 4] = y2;
        allInputs[inputSetIdx, 5] = y3;

        var result = x1*y1 + x2*y2 + x3*y3;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "x3", "y1", "y2", "y3"];
    }
    #endregion
}