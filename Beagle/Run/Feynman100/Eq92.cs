using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq92 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;   
        var h = 1 + Rnd.Random.NextSingle() * 4;   
        
        inputs[0] = n;
        inputs[1] = h;
        
        var result = n * h;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "h"];
    }
    #endregion
}