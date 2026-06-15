using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq74 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges: {1,5} for eps and E
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var e = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = eps;
        inputs[1] = e;

        var result = eps * e * e;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "E"];
    }
    #endregion
}
