using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq90 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var p = 1 + Rnd.Random.NextSingle() * 2;
        var e = 1 + Rnd.Random.NextSingle() * 2;
        var t = 1 + Rnd.Random.NextSingle() * 2;
        var h = 1 + Rnd.Random.NextSingle() * 2;
        var w = 1 + Rnd.Random.NextSingle() * 4;
        var w0 = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = p;
        allInputs[inputSetIdx, 1] = e;
        allInputs[inputSetIdx, 2] = t;
        allInputs[inputSetIdx, 3] = h;
        allInputs[inputSetIdx, 4] = w;
        allInputs[inputSetIdx, 5] = w0;
        
        var term = (w - w0) * t / 2f;
        var sinTerm = MathF.Sin(term);

        var result = (p * e * t / h) * (sinTerm*sinTerm / (term*term));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "E", "t", "h", "w", "w0"];
    }
    #endregion
}


   