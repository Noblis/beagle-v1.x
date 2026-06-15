using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq46 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = u;
	    inputs[1] = k;
	    inputs[2] = t;

        var result = u*k*t;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u","k","t"];
    }
    #endregion
}
