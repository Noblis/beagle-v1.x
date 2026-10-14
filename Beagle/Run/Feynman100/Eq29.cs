using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq29 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
	    var x2 = 1 + Rnd.Random.NextSingle() * 4;
	    var theta1 = 1 + Rnd.Random.NextSingle() * 4;
	    var theta2 = 1 + Rnd.Random.NextSingle() * 4;
        
	    allInputs[inputSetIdx, 0] = x1;
	    allInputs[inputSetIdx, 1] = x2;
	    allInputs[inputSetIdx, 2] = theta1;
	    allInputs[inputSetIdx, 3] = theta2;

        var result = MathF.Sqrt(x1*x1 + x2*x2 - 2*x1* x2*MathF.Cos(theta1-theta2));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1","x2","Theta1", "Theta2"];
    }
    #endregion
}
