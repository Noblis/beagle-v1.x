using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq41 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var y = 2 + Rnd.Random.NextSingle() * 3;
        var p = 1 + Rnd.Random.NextSingle() * 4;
	    var gamma = 1 + Rnd.Random.NextSingle() * 4;
	    
	    allInputs[inputSetIdx, 0] = y;
	    allInputs[inputSetIdx, 1] = p;
	    allInputs[inputSetIdx, 2] = gamma;
	    
        var result = 1f/(y-1f)*p*gamma;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["y","p","gamma"];
    }
    #endregion
}
