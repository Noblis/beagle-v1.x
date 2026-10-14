using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq53 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var p = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = p;
        allInputs[inputSetIdx, 1] = r;
    
        var result = p/ (4f * MathF.PI * r*r);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["P", "r"];
    }
    #endregion
}
