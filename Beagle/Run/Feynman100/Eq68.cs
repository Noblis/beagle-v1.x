using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq68 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // random input generation (ranges shown in comments)
        var p = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var v = Rnd.Random.NextSingle() * 1 + 1; // 0..1
        var c = Rnd.Random.NextSingle() * 7 + 3; // 3..10

        allInputs[inputSetIdx, 0] = p; 
        allInputs[inputSetIdx, 1] = v; 
        allInputs[inputSetIdx, 2] = c;

        var result = p*v / MathF.Sqrt(1f - v*v/(c*c));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "v", "c"];
    }
    #endregion
}
