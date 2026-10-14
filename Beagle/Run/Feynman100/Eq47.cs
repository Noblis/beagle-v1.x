using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq47 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var gamma = 2 + Rnd.Random.NextSingle() * 3;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        
	    allInputs[inputSetIdx, 0] = gamma;
	    allInputs[inputSetIdx, 1] = k;
        allInputs[inputSetIdx, 2] = v;
        allInputs[inputSetIdx, 3] = a;

        var result = (1f/(gamma-1f)) * (k*v/a);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["gamma","k","v","a"];
    }
    #endregion
}
