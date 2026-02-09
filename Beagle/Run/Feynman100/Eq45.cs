using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq45 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var u = 1 + Rnd.Random.NextSingle() * 4;
	    var q = 1 + Rnd.Random.NextSingle() * 4;
        var v = 1 + Rnd.Random.NextSingle() * 4;
	    var d = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = u;
	    inputs[1] = q;
	    inputs[2] = v;
	    inputs[3] = d;

        var result = u*q*v/d;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u","q","v","d"];
    }
    #endregion
}
