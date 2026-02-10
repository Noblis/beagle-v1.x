using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq76 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
    
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q;
        inputs[1] = v;
        inputs[2] = r;

    
        var result = q * v * r / 2f;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "v", "r"];
    }
    #endregion
}