using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq83 : FeynmanMLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = u;
        inputs[1] = x;
        inputs[2] = b;

        var result = u * (1f + x) * b;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "x", "B"];
    }
    #endregion
}

