using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq50 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
        var v = 1 + Rnd.Random.NextSingle() * 1;
        
	    inputs[0] = m;
	    inputs[1] = c;
        inputs[2] = v;

        var result = m*c*c/MathF.Sqrt(1f-v*v/(c*c));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m","c","v"];
    }
    #endregion
}
