using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq75 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var v = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = q;
        inputs[1] = v;
        inputs[2] = r;

        var result = q * v / (2f * MathF.PI * r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "v", "r"];
    }
    #endregion
}
