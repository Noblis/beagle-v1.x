using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq53 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var p = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = p;
        inputs[1] = r;
    
        var result = p/ (4f * MathF.PI * r*r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["P", "r"];
    }
    #endregion
}
