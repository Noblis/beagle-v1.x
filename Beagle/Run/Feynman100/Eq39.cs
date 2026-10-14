using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq39 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var eps = 1 + Rnd.Random.NextSingle() * 4;
	    var h = 1 + Rnd.Random.NextSingle() * 4;
	    var m = 1 + Rnd.Random.NextSingle() * 4;
	    var q = 1 + Rnd.Random.NextSingle() * 4;
	    
	    allInputs[inputSetIdx, 0] = eps;
	    allInputs[inputSetIdx, 1] = h;
	    allInputs[inputSetIdx, 2] = m;
	    allInputs[inputSetIdx, 3] = q;
	    
        var result = (4f*MathF.PI*eps*h*h)/(m*q*q);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps","h","m","q"];
    }
    #endregion
}
