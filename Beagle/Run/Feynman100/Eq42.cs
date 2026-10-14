using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq42 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = n;
	    allInputs[inputSetIdx, 1] = k;
	    allInputs[inputSetIdx, 2] = t;
	    allInputs[inputSetIdx, 3] = v;

        var result = n*k*t/v;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","k","T","v"];
    }
    #endregion
}
