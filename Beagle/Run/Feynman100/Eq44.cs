using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq44 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = h;
	    inputs[1] = w;
	    inputs[2] = c;
	    inputs[3] = k;
	    inputs[4] = t;

        var result = (h*w*w*w)/(MathF.PI*MathF.PI*c*c*(MathF.Exp((h*w)/(k*t))-1));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h","w","c","k","T"];
    }
    #endregion
}
