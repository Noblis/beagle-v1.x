using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq40 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var p = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = p;
	    inputs[1] = v;
	    
        var result = 3f/2f*p*v;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p","v"];
    }
    #endregion
}
