using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq13 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var ef = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = q;
        inputs[1] = ef;
        inputs[2] = b;
        inputs[3] = v;
        inputs[4] = theta;

        var result = q * (ef + b * v * MathF.Sin(theta));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Ef", "B", "v", "Theta"];
    }
    #endregion
}