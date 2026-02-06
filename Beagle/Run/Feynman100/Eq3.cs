using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq3 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var theta = 1 + Rnd.Random.NextSingle() * 2;
        var theta1 = 1 + Rnd.Random.NextSingle() * 2;
        var sigma = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = theta;
        inputs[1] = theta1;
        inputs[2] = sigma;
        // Checked

        var tmp = (theta - theta1) / sigma;
        var result = MathF.Exp(-tmp*tmp / 2f) / (MathF.Sqrt(2f * MathF.PI) * sigma);
        //var result = MathF.Exp(-MathF.Pow((x1 - x2) / x3, 2) / 2f) / (MathF.Sqrt(2f * MathF.PI) * x3);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Theta", "Theta1", "Sigma"];
    }
    #endregion
}
