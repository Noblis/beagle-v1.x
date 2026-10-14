using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq43 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var m = 1 + Rnd.Random.NextSingle() * 4;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    var x = 1 + Rnd.Random.NextSingle() * 4;
        var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
        
	    allInputs[inputSetIdx, 0] = n;
	    allInputs[inputSetIdx, 1] = m;
	    allInputs[inputSetIdx, 2] = w;
	    allInputs[inputSetIdx, 3] = x;
	    allInputs[inputSetIdx, 4] = k;
	    allInputs[inputSetIdx, 5] = t;

        var result = n*MathF.Exp(m*w*x/(k*t));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","m","w","x","k","t"];
    }
    #endregion
}
