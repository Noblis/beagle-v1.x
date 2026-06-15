using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq41 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var y = 2 + Rnd.Random.NextSingle() * 3;
        var p = 1 + Rnd.Random.NextSingle() * 4;
	    var gamma = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = y;
	    inputs[1] = p;
	    inputs[2] = gamma;
	    
        var result = 1f/(y-1f)*p*gamma;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["y","p","gamma"];
    }
    #endregion
}
