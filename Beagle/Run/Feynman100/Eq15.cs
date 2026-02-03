using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq15 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var z = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = m;
        inputs[1] = g;
        inputs[2] = z;

        var result = m * g * z;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "g", "z"];
    }
    #endregion
}