using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq10 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q1 = 1 + Rnd.Random.NextSingle() * 4;
        var q2 = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;
        var eps = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q1;
        inputs[1] = q2;
        inputs[2] = r;
        inputs[3] = eps;

        var result = (q1 * q2 * r) / (4f * MathF.PI * eps * r * r * r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q1", "q2", "r", "Eps"];
    }
    #endregion
}