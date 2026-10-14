using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq96 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var e = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = h;
        allInputs[inputSetIdx, 1] = e;
        allInputs[inputSetIdx, 2] = d;
        
        var result = h * h / (2f * e * d*d);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h", "e", "d"];
    }
    #endregion
}