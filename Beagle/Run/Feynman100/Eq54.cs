using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq54 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = q;
        inputs[1] = eps;
        inputs[2] = r;
    
        var result = q/(4f * MathF.PI * eps * r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Eps", "r"];
    }
    #endregion
}
