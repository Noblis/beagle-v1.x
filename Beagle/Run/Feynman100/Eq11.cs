using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq11 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q1 = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;
        var eps = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q1;
        inputs[1] = r;
        inputs[2] = eps;

        var result = (q1 * r) / (4f * MathF.PI * eps * r * r * r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q1", "r", "eps"];
    }
    #endregion
}