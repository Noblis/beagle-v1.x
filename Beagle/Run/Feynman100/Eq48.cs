using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq48 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    var v2 = 1 + Rnd.Random.NextSingle() * 4;
        var v1 = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = n;
	    inputs[1] = k;
        inputs[2] = t;
        inputs[3] = v2;
        inputs[4] = v1;
        
        var result = n*k*t*MathF.Log(v2/v1);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","k","T","v2","v1"];
    }
    #endregion
}
