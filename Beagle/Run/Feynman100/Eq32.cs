using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq32 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = 1 + Rnd.Random.NextSingle() * 4;
	    var a = 1 + Rnd.Random.NextSingle() * 4;
	    var eps = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = q;
	    inputs[1] = a;
	    inputs[2] = eps;
	    inputs[3] = c;
	    
        var result = q*q*a*a/(6 * MathF.PI * eps * c*c*c);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q","a","eps","c"];
    }
    #endregion
}
