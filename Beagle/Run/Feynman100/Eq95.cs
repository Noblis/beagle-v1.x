using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq95 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
        allInputs[inputSetIdx, 1] = k;
        allInputs[inputSetIdx, 2] = d;
        
        var result = 2f * u * (1f - MathF.Cos(k * d));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["U", "k", "d"];
    }
    #endregion
}
