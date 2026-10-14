using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq61 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        // random input generation (ranges shown in comments)
        var q = Rnd.Random.NextSingle() * 2 + 1; // range 1..3
        var e = Rnd.Random.NextSingle() * 2 + 1; // range 1..3
        var m = Rnd.Random.NextSingle() * 2 + 1; // range 3..5
        var w0 = Rnd.Random.NextSingle() * 2 + 3; // range 1..2
        var w = Rnd.Random.NextSingle() * 1 + 1; // range 1..2

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = e;
        allInputs[inputSetIdx, 2] = m;
        allInputs[inputSetIdx, 3] = w0;
        allInputs[inputSetIdx, 4] = w;

        var result = (q * e) / (m * (w0*w0 + w*w));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "E", "m", "w0", "w"];
    }
    #endregion
}
