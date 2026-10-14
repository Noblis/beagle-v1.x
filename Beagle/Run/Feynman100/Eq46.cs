using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq46 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = u;
	    allInputs[inputSetIdx, 1] = k;
	    allInputs[inputSetIdx, 2] = t;

        var result = u*k*t;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u","k","t"];
    }
    #endregion
}
