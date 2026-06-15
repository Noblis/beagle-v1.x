using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq96 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
        var e = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;
        
        inputs[0] = h;
        inputs[1] = e;
        inputs[2] = d;
        
        var result = h * h / (2f * e * d*d);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h", "e", "d"];
    }
    #endregion
}