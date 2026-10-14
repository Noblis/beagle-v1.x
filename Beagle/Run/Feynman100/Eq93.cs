using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq93 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var e = 1 + Rnd.Random.NextSingle() * 4;   
        var d = 1 + Rnd.Random.NextSingle() * 4;   
        var k = 1 + Rnd.Random.NextSingle() * 4;   
        var h = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = e;
        allInputs[inputSetIdx, 1] = d;
        allInputs[inputSetIdx, 2] = k;
        allInputs[inputSetIdx, 3] = h;
        
        var result = 2f * e * d*d * k / h;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["E", "d", "k", "h"];
    }
    #endregion
}
