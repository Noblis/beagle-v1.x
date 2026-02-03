using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq26 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var n = 0 + Rnd.Random.NextSingle() * 1;
	    var theta = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = n;
	    inputs[1] = theta;

        var result = MathF.Asin(n * MathF.Sin(theta));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","Theta"];
    }
    #endregion
}
