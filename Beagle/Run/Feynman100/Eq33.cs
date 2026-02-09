using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq33 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var eps = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 1 + Rnd.Random.NextSingle() * 1;
	    var e = 1 + Rnd.Random.NextSingle() * 1;
	    var r = 1 + Rnd.Random.NextSingle() * 1;
	    var w = 1 + Rnd.Random.NextSingle() * 1;
	    var w1 = 3 + Rnd.Random.NextSingle() * 2;
	    
	    inputs[0] = eps;
	    inputs[1] = c;
	    inputs[2] = e;
	    inputs[3] = r;
	    inputs[4] = w;
	    inputs[5] = w1;

        var tmp = w*w - w1*w1;
        var result = (1f/2f*eps*c*e*e) * (8*MathF.PI*r*r/3) * (w*w*w*w/tmp);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps","c","E","r","w","w1"];
    }
    #endregion
}
