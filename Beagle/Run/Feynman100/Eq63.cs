using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq63 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
	    var n = Rnd.Random.NextSingle() * 4 + 1;
	    var p = Rnd.Random.NextSingle() * 4 + 1;	
        var e = Rnd.Random.NextSingle() * 4 + 1;
        var k = Rnd.Random.NextSingle() * 4 + 1;
        var t = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = n;
        allInputs[inputSetIdx, 1] = p;
        allInputs[inputSetIdx, 2] = e;
	    allInputs[inputSetIdx, 3] = k;
	    allInputs[inputSetIdx, 4] = t;
        
        // f = (3/5) * ( q^2 / (4*pi*eps*d) )
        var result = (n * p * p * e) / (3f * k * t) ;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "p", "E", "k", "T"];
    }
    #endregion
}
