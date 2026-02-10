using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq77 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
   
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;
      
        inputs[0] = g;
        inputs[1] = q;
        inputs[2] = b;
        inputs[3] = m;
        
        var result = g * q * b / (2.0f * m);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["g", "q", "B", "m"];
    }
    #endregion
}

