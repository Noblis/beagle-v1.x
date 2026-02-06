using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq30 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var l = 1 + Rnd.Random.NextSingle() * 4;
	    var n = 1 + Rnd.Random.NextSingle() * 4;
	    var theta = 1 + Rnd.Random.NextSingle() * 4;
	    
	    inputs[0] = l;
	    inputs[1] = n;
	    inputs[2] = theta;

        var enumerator = MathF.Sin(n * theta / 2);
        var denominator = MathF.Sin(theta / 2);
        var result = l * enumerator * enumerator / (denominator * denominator);
        
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["L","n","Theta"];
    }
    #endregion
}
