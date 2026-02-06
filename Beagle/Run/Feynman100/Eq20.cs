using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq20 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var c = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = u;
        inputs[1] = v;
        inputs[2] = c;

        var result = (u + v) / (1f + (u * v / (c*c)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "v", "c"];
    }
    #endregion
}