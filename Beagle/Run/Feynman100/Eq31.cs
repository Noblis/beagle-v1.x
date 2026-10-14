using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq31 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var lambda = 1 + Rnd.Random.NextSingle() * 1;
	    var n = 1 + Rnd.Random.NextSingle() * 4;
	    var d = 2 + Rnd.Random.NextSingle() * 3;

        allInputs[inputSetIdx, 0] = lambda;
	    allInputs[inputSetIdx, 1] = n;
	    allInputs[inputSetIdx, 2] = d;
	    
        var result = MathF.Asin(lambda/(n*d));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Lambda","n","d"];
    }
    #endregion
}
