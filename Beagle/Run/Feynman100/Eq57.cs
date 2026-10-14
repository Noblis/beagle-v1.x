using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq57 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var eps = Rnd.Random.NextSingle() * 2 + 1;
        var p = Rnd.Random.NextSingle() * 2 + 1;
        var r = Rnd.Random.NextSingle() * 2 + 1;
        var theta = Rnd.Random.NextSingle() * 2 + 1;

        allInputs[inputSetIdx, 0] = eps;
        allInputs[inputSetIdx, 1] = p;
        allInputs[inputSetIdx, 2] = r;
        allInputs[inputSetIdx, 3] = theta;

        var denominator1 = 4f * MathF.PI * eps;
        var denominator2 = r*r*r * MathF.Cos(theta) * MathF.Sin(theta);
        var result = (3f / denominator1) * (p / denominator2);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "p", "r", "Theta"];
    }
    #endregion
}
