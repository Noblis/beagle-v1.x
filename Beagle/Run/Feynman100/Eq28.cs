using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq28 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var w = 1 + Rnd.Random.NextSingle() * 9;
	    var c = 1 + Rnd.Random.NextSingle() * 9;
        
	    inputs[0] = w;
	    inputs[1] = c;

        var result = w/c;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["w","c"];
    }
    #endregion
}
