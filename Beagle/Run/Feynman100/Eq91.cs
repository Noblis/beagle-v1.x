using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq91 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u  = 1 + Rnd.Random.NextSingle() * 4;   
        var bx = 1 + Rnd.Random.NextSingle() * 4;   
        var by = 1 + Rnd.Random.NextSingle() * 4;   
        var bz = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
        allInputs[inputSetIdx, 1] = bx;
        allInputs[inputSetIdx, 2] = by;
        allInputs[inputSetIdx, 3] = bz;

        var result = u * MathF.Sqrt(bx*bx + by*by + bz*bz);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "Bx", "By", "Bz"];
    }
    #endregion
}