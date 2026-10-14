using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq89 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var e = 1 + Rnd.Random.NextSingle() * 1;   
        var t = 1 + Rnd.Random.NextSingle() * 1;   
        var h = 1 + Rnd.Random.NextSingle() * 3;

        allInputs[inputSetIdx, 0] = e;
        allInputs[inputSetIdx, 1] = t;
        allInputs[inputSetIdx, 2] = h;

        var tmp = MathF.Sin(e * t / h);
        var result = tmp*tmp;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["e", "t", "h"];
    }
    #endregion
}