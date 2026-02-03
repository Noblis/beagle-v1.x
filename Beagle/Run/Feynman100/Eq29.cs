using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq29 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
	    var x2 = 1 + Rnd.Random.NextSingle() * 4;
	    var theta1 = 1 + Rnd.Random.NextSingle() * 4;
	    var theta2 = 1 + Rnd.Random.NextSingle() * 4;
        
	    inputs[0] = x1;
	    inputs[1] = x2;
	    inputs[2] = theta1;
	    inputs[3] = theta2;

        var result = MathF.Sqrt(x1*x1 + x2*x2 - 2*x1* x2*MathF.Cos(theta1-theta2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1","x2","Theta1", "Theta2"];
    }
    #endregion
}
