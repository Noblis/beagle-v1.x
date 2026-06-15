using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq47 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var gamma = 2 + Rnd.Random.NextSingle() * 3;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 4;
        var a = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = gamma;
	    inputs[1] = k;
        inputs[2] = v;
        inputs[3] = a;

        var result = (1f/(gamma-1f)) * (k*v/a);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["gamma","k","v","a"];
    }
    #endregion
}
