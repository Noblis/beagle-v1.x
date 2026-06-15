using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq60 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var sigma = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var chi = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = sigma;
        inputs[1] = eps;
        inputs[2] = chi;

        var result = (sigma / eps) * (1f / (1f + chi));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Sigma", "Eps", "Chi"];
    }
    #endregion
}
