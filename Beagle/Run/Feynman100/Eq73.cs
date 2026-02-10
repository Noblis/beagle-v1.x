using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq73 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var c = Rnd.Random.NextSingle() * 4 + 1;
        var e = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = eps;
        inputs[1] = c;
        inputs[2] = e;

        // f = eps * c * E^2
        var result = eps * c * e*e;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "c", "E"];
    }
    #endregion
}
