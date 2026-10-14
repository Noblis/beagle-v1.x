using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq92 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;   
        var h = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = n;
        allInputs[inputSetIdx, 1] = h;
        
        var result = n * h;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "h"];
    }
    #endregion
}