using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq97 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var alpha = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = alpha;
        inputs[1] = n;
        inputs[2] = d;

        var result = 2f * MathF.PI * alpha / (n * d);

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Alpha", "n", "d"];
    }
    #endregion
}
