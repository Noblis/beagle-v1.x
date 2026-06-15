using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq89 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var e = 1 + Rnd.Random.NextSingle() * 1;   
        var t = 1 + Rnd.Random.NextSingle() * 1;   
        var h = 1 + Rnd.Random.NextSingle() * 3;   
        
        inputs[0] = e;
        inputs[1] = t;
        inputs[2] = h;

        var tmp = MathF.Sin(e * t / h);
        var result = tmp*tmp;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["e", "t", "h"];
    }
    #endregion
}