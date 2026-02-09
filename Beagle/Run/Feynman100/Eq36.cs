using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq36 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var v = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = v;
	    inputs[1] = c;
	    inputs[2] = w;
	    
        var result = (1f+v/c)/(MathF.Sqrt(1f-v*v/(c*c)))*w;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["v","c","w"];
    }
    #endregion
}
