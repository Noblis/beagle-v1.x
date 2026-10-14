using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq27 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var d1 = 1 + Rnd.Random.NextSingle() * 4;
        var d2 = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = d1;
        allInputs[inputSetIdx, 1] = d2;
	    allInputs[inputSetIdx, 2] = n;

        var result = 1/(1/d1+n/d2);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","d1","d2"];
    }
    #endregion
}
