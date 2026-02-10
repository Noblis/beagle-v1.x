using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq95 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;
        
        inputs[0] = u;
        inputs[1] = k;
        inputs[2] = d;
        
        var result = 2f * u * (1f - MathF.Cos(k * d));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["U", "k", "d"];
    }
    #endregion
}
