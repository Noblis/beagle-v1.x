using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq18 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var t = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 1;
        var c = 3 + Rnd.Random.NextSingle() * 7;
        var x = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = t;
        inputs[1] = u;
        inputs[2] = c;
        inputs[3] = x;
        // Checked
        var result = (t - u*x / (c*c)) / MathF.Sqrt(1f - (u*u) / (c*c));
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["t", "u", "c", "x"];
    }
    #endregion
}