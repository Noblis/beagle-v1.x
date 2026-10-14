using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq4 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var y1 = 1 + Rnd.Random.NextSingle() * 4;
        var y2 = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = x1;
        allInputs[inputSetIdx, 1] = x2;
        allInputs[inputSetIdx, 2] = y1;
        allInputs[inputSetIdx, 3] = y2;

        var result = MathF.Sqrt((x2 - x1)*(x2 - x1) + (y2 - y1)*(y2 - y1));
        //var result = MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(x4 - x3, 2));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "y1", "y2"];
    }
    #endregion
}
