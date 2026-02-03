using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq12 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q2 = 1 + Rnd.Random.NextSingle() * 4;
        var e = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q2;
        inputs[1] = e;

        var result = q2 * e;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q2", "E"];
    }
    #endregion
}