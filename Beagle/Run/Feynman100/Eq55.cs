using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq55 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var eps = Rnd.Random.NextSingle() * 2 + 1;
        var p = Rnd.Random.NextSingle() * 2 + 1;
        var theta = Rnd.Random.NextSingle() * 2 + 1;
        var r = Rnd.Random.NextSingle() * 2 + 1;

        allInputs[inputSetIdx, 0] = eps;
        allInputs[inputSetIdx, 1] = p;
        allInputs[inputSetIdx, 2] = theta;
        allInputs[inputSetIdx, 3] = r;
    
        var result = 1f / ( 4f * MathF.PI * eps) * (p * MathF.Cos(theta)/ r*r);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "p", "Theta", "r"];
    }
    #endregion
}
