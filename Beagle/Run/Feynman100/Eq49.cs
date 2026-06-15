using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq49 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var gamma = 1 + Rnd.Random.NextSingle() * 4;
	    var pr = 1 + Rnd.Random.NextSingle() * 4;
        var p = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = gamma;
	    inputs[1] = pr;
	    inputs[2] = p;

        var result = MathF.Sqrt(gamma*pr/p);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["gamma","pr","p"];
    }
    #endregion
}
