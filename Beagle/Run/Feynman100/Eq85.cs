using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq85 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var y = 1 + Rnd.Random.NextSingle() * 4;
        var sigma = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = y;
        inputs[1] = sigma;

        var result = y / (2f * (1f + sigma));

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Y", "sigma"];
    }
    #endregion
}

