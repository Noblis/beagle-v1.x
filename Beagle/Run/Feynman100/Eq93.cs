using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq93 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var e = 1 + Rnd.Random.NextSingle() * 4;   
        var d = 1 + Rnd.Random.NextSingle() * 4;   
        var k = 1 + Rnd.Random.NextSingle() * 4;   
        var h = 1 + Rnd.Random.NextSingle() * 4;   
        
        inputs[0] = e;
        inputs[1] = d;
        inputs[2] = k;
        inputs[3] = h;
        
        var result = 2f * e * d*d * k / h;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["E", "d", "k", "h"];
    }
    #endregion
}
