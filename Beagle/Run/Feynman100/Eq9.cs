using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq9 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var w = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = m;
        allInputs[inputSetIdx, 1] = v;
        allInputs[inputSetIdx, 2] = u;
        allInputs[inputSetIdx, 3] = w;

        var result = 0.5f * m * (v*v + u*u + w*w);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "v", "u", "w"];
    }
    #endregion
}