using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq28 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var w = 1 + Rnd.Random.NextSingle() * 9;
	    var c = 1 + Rnd.Random.NextSingle() * 9;
        
	    allInputs[inputSetIdx, 0] = w;
	    allInputs[inputSetIdx, 1] = c;

        var result = w/c;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["w","c"];
    }
    #endregion
}
