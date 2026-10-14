using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq54 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = eps;
        allInputs[inputSetIdx, 2] = r;
    
        var result = q/(4f * MathF.PI * eps * r);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Eps", "r"];
    }
    #endregion
}
