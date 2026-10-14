using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq50 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        
	    allInputs[inputSetIdx, 0] = m;
	    allInputs[inputSetIdx, 1] = c;
        allInputs[inputSetIdx, 2] = v;

        var result = m*c*c/MathF.Sqrt(1f-v*v/(c*c));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m","c","v"];
    }
    #endregion
}
