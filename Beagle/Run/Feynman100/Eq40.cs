using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq40 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var p = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = p;
        allInputs[inputSetIdx, 1] = v;
	    
        var result = 3f/2f*p*v;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p","v"];
    }
    #endregion
}
