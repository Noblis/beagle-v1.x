using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq58 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var d = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = q;
        inputs[1] = eps;
        inputs[2] = d;

        var result = (3f / 5f) * (q * q / (4f * MathF.PI * eps * d));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Eps", "d"];
    }
    #endregion
}
