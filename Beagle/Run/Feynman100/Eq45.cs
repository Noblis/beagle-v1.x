using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq45 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
	    var q = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
	    var d = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
	    allInputs[inputSetIdx, 1] = q;
	    allInputs[inputSetIdx, 2] = v;
	    allInputs[inputSetIdx, 3] = d;

        var result = u*q*v/d;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u","q","v","d"];
    }
    #endregion
}
