using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq88 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
        
        inputs[0] = u;
        inputs[1] = b;
        inputs[2] = h;
       
        var result = 2f * u * b / h;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "B", "h"];
    }
    #endregion
}

