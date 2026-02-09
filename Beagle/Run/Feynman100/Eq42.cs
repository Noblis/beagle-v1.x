using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq42 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = n;
	    inputs[1] = k;
	    inputs[2] = t;
	    inputs[3] = v;

        var result = n*k*t/v;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","k","T","v"];
    }
    #endregion
}
