using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq77 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
   
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var q = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var m = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = g;
        allInputs[inputSetIdx, 1] = q;
        allInputs[inputSetIdx, 2] = b;
        allInputs[inputSetIdx, 3] = m;
        
        var result = g * q * b / (2.0f * m);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["g", "q", "B", "m"];
    }
    #endregion
}

