using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq8 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var mu = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = mu;
        inputs[1] = n;

        var result = mu * n;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Mu", "N"];
    }
    #endregion
}