using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq39 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var eps = 1 + Rnd.Random.NextSingle() * 4;
	    var h = 1 + Rnd.Random.NextSingle() * 4;
	    var m = 1 + Rnd.Random.NextSingle() * 4;
	    var q = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = eps;
	    inputs[1] = h;
	    inputs[2] = m;
	    inputs[3] = q;
	    
        var result = (4f*MathF.PI*eps*h*h)/(m*q*q);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps","h","m","q"];
    }
    #endregion
}
