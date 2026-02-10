using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq94 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var l = 1 + Rnd.Random.NextSingle() * 4;   
        var q = 1 + Rnd.Random.NextSingle() * 1;   
        var v = 1 + Rnd.Random.NextSingle() * 1;   
        var k = 1 + Rnd.Random.NextSingle() * 1;   
        var t = 1 + Rnd.Random.NextSingle() * 1;   
        
        inputs[0] = l;
        inputs[1] = q;
        inputs[2] = v;
        inputs[3] = k;
        inputs[4] = t;
        
        var result = l * (MathF.Exp(q * v / (k * t)) - 1f);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["l", "q", "V", "k", "T"];
    }
    #endregion
}