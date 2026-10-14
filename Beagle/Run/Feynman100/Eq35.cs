using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq35 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var w = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 3 + Rnd.Random.NextSingle() * 7;

        allInputs[inputSetIdx, 0] = w;
        allInputs[inputSetIdx, 1] = v;
        allInputs[inputSetIdx, 2] = c;
	    
        var result = w/(1f-v/c);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["w","v","c"];
    }
    #endregion
}
