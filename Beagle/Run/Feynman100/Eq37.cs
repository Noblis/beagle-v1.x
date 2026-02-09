using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq37 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = h;
	    inputs[1] = w;
	    
        var result = h*w;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h","w"];
    }
    #endregion
}
