using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq23 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
        var r = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = m;
        inputs[1] = r;
        inputs[2] = v;
        inputs[3] = theta;

        var result = m * r * v * MathF.Sin(theta);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m", "r", "v", "Theta"];
    }
    #endregion
}