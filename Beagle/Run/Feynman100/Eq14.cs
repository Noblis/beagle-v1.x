using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq14 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var m1 = 1 + Rnd.Random.NextSingle() * 4;
        var m2 = 1 + Rnd.Random.NextSingle() * 4;
        var r1 = 1 + Rnd.Random.NextSingle() * 4;
        var r2 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = g;
        inputs[1] = m1;
        inputs[2] = m2;
        inputs[3] = r1;
        inputs[4] = r2;

        var result = g * m1 * m2 * ((1f / (r2*r2)) - (1f / (r1*r1)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["G", "m1", "m2", "r1", "r2"];
    }
    #endregion
}