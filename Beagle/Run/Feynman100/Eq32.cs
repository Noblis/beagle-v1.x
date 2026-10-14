using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq32 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
	    var a = 1 + Rnd.Random.NextSingle() * 4;
	    var eps = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = q;
	    allInputs[inputSetIdx, 1] = a;
	    allInputs[inputSetIdx, 2] = eps;
	    allInputs[inputSetIdx, 3] = c;
	    
        var result = q*q*a*a/(6 * MathF.PI * eps * c*c*c);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q","a","Eps","c"];
    }
    #endregion
}
