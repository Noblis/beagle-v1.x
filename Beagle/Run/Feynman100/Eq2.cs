using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq2 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var theta = 1 + Rnd.Random.NextSingle() * 2;
        var sigma = 1 + Rnd.Random.NextSingle() * 2;

        allInputs[inputSetIdx, 0] = theta;
        allInputs[inputSetIdx, 1] = sigma;

        // Checked    
        var result = MathF.Exp(-(theta / sigma) * (theta / sigma) / 2f) / (MathF.Sqrt(2f * MathF.PI) * sigma);
        //var result = MathF.Exp(-MathF.Pow(x1 / x2, 2) / 2f) / (MathF.Sqrt(2f * MathF.PI) * x2);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Theta", "Sigma"];
    }
    #endregion
}
