using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq91 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u  = 1 + Rnd.Random.NextSingle() * 4;   
        var bx = 1 + Rnd.Random.NextSingle() * 4;   
        var by = 1 + Rnd.Random.NextSingle() * 4;   
        var bz = 1 + Rnd.Random.NextSingle() * 4;   
        
        inputs[0] = u;
        inputs[1] = bx;
        inputs[2] = by;
        inputs[3] = bz;

        var result = u * MathF.Sqrt(bx*bx + by*by + bz*bz);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "Bx", "By", "Bz"];
    }
    #endregion
}