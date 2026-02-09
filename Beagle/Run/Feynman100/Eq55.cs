using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq55 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var eps = Rnd.Random.NextSingle() * 2 + 1;
        var p = Rnd.Random.NextSingle() * 2 + 1;
        var theta = Rnd.Random.NextSingle() * 2 + 1;
        var r = Rnd.Random.NextSingle() * 2 + 1;

        inputs[0] = eps;
        inputs[1] = p;
        inputs[2] = theta;
        inputs[3] = r;
    
        var result = 1f / ( 4f * MathF.PI * eps) * (p * MathF.Cos(theta)/ r*r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["eps", "p", "theta", "r"];
    }
    #endregion
}
