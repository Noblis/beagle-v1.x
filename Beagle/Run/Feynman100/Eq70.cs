using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq70 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // random input generation (ranges shown in comments)
        var p = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var e = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var theta = Rnd.Random.NextSingle() * 4 + 1; // 1..5 rad

        allInputs[inputSetIdx, 0] = p; 
        allInputs[inputSetIdx, 1] = e; 
        allInputs[inputSetIdx, 2] = theta;

        // f = -p * E * Cos(theta)
        var result = -p * e * MathF.Cos(theta);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "E", "Theta"];
    }
    #endregion
}
