using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq76 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
    
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = v;
        allInputs[inputSetIdx, 2] = r;
    
        var result = q * v * r / 2f;

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "v", "r"];
    }
    #endregion
}