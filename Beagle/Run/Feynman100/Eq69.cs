using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq69 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // random input generation (ranges shown in comments)
        var u = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var b = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var theta = Rnd.Random.NextSingle() * 4 + 1; // 1..5 rad

        allInputs[inputSetIdx, 0] = u; 
        allInputs[inputSetIdx, 1] = b; 
        allInputs[inputSetIdx, 2] = theta;

        var result = -u * b * MathF.Cos(theta);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "B", "Theta"];
    }
    #endregion
}
