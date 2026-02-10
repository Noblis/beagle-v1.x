using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq100 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var p = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = p;
        inputs[1] = q;
        inputs[2] = a;
        inputs[3] = m;

        var result = -p * q * a / m;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "q", "A", "m"];
    }
    #endregion
}
