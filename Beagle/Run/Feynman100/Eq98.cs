using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq98 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var beta = 1 + Rnd.Random.NextSingle() * 4;   
        var alpha = 1 + Rnd.Random.NextSingle() * 4;  
        var theta = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = beta;
        allInputs[inputSetIdx, 1] = alpha;
        allInputs[inputSetIdx, 2] = theta;

        var result = beta * (1f + alpha * MathF.Cos(theta));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Beta", "Alpha", "Theta"];
    }
    #endregion
}
