using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq35 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var w = 1 + Rnd.Random.NextSingle() * 4;
	    var v = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
	    
	    inputs[0] = w;
	    inputs[1] = v;
	    inputs[2] = c;
	    
        var result = w/(1f-v/c);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["w","v","c"];
    }
    #endregion
}
