using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq62 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var p = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var e = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var theta = Rnd.Random.NextSingle() * 2 + 1; // 1..3 (rad)
        var k = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var t = Rnd.Random.NextSingle() * 2 + 1; // 1..3

        allInputs[inputSetIdx, 0] = n; 
        allInputs[inputSetIdx, 1] = p; 
        allInputs[inputSetIdx, 2] = e; 
        allInputs[inputSetIdx, 3] = theta; 
        allInputs[inputSetIdx, 4] = k; 
        allInputs[inputSetIdx, 5] = t;

        // f = n * (1 + (p * E * Cos[theta])/(k*T))
        var result = n * (1f + p * e * MathF.Cos(theta)/(k * t));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "p", "E", "Theta", "k", "T"];
    }
    #endregion
}
