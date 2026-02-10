using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq84 : FeynmanMLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var y = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;
        var d = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = y;
        inputs[1] = a;
        inputs[2] = x;
        inputs[3] = d;

        var result = y * a * x / d;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Y", "A", "x", "d"];
    }
    #endregion
}

