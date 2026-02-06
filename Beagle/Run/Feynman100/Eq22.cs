using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq22 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var r = 1 + Rnd.Random.NextSingle() * 4;
        var f = 1 + Rnd.Random.NextSingle() * 4;
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = r;
        inputs[1] = f;
        inputs[2] = theta;

        var result = r * f * MathF.Sin(theta);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["r", "f", "Theta"];
    }
    #endregion
}