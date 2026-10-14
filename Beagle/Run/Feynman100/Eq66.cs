using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq66 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // random input generation (ranges shown in comments)
        var eps = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var c = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var l = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var r = Rnd.Random.NextSingle() * 4 + 1; // 1..5

        allInputs[inputSetIdx, 0] = eps; 
        allInputs[inputSetIdx, 1] = c; 
        allInputs[inputSetIdx, 2] = l; 
        allInputs[inputSetIdx, 3] = r;

        var result = 1f / (4f * MathF.PI * eps * c*c) * (2f*l / r);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "c", "l", "r"];
    }
    #endregion
}
