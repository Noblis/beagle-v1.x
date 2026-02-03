using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq25 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var c = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q;
        inputs[1] = c;

        var result = q / c;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "c"];
    }
    #endregion
}