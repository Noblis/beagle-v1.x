using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq37 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    
	    allInputs[inputSetIdx, 0] = h;
	    allInputs[inputSetIdx, 1] = w;
	    
        var result = h*w;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h","w"];
    }
    #endregion
}
