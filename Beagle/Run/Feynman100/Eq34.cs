using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq34 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
	    var b = 1 + Rnd.Random.NextSingle() * 4;
	    var p = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = q;
	    inputs[1] = v;
	    inputs[2] = b;
	    inputs[3] = p;
	    
        var result = q*v*b/p;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q","v","B","p"];
    }
    #endregion
}
