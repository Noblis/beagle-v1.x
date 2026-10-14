using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq5 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var g = 1 + Rnd.Random.NextSingle() * 1;
        var m1 = 1 + Rnd.Random.NextSingle() * 1;
        var m2 = 1 + Rnd.Random.NextSingle() * 1;
        var x1 = 3 + Rnd.Random.NextSingle() * 1;
        var x2 = 1 + Rnd.Random.NextSingle() * 1;
        var y1 = 3 + Rnd.Random.NextSingle() * 1;
        var y2 = 1 + Rnd.Random.NextSingle() * 1;
        var z1 = 3 + Rnd.Random.NextSingle() * 1;
        var z2 = 1 + Rnd.Random.NextSingle() * 1;

        allInputs[inputSetIdx, 0] = g;
        allInputs[inputSetIdx, 1] = m1;
        allInputs[inputSetIdx, 2] = m2;
        allInputs[inputSetIdx, 3] = x1;
        allInputs[inputSetIdx, 4] = x2;
        allInputs[inputSetIdx, 5] = y1;
        allInputs[inputSetIdx, 6] = y2;
        allInputs[inputSetIdx, 7] = z1;
        allInputs[inputSetIdx, 8] = z2;

        var dx = x2 - x1;
        var dy = y2 - y1;
        var dz = z2 - z1;
        var result = (g * m1 * m2) / (dx*dx + dy*dy + dz*dz);
        //var result = (g * m1 * m2) / (MathF.Pow(x2 - x1, 2) + MathF.Pow(y2 - y1, 2) + MathF.Pow(z2 - z1, 2));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["G", "m1", "m2", "x1", "x2", "y1", "y2", "z1", "z2"];
    }
    #endregion
}