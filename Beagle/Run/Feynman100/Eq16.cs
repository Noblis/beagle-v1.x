using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq16 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var k = 1 + Rnd.Random.NextSingle() * 4;
        var x = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = k;
        inputs[1] = x;

        var result = 0.5f * k * x * x;
        return (inputs, result);
    }

    public override string[] GetInputLabels()
    {
        return ["k", "x"];
    }
    #endregion
}