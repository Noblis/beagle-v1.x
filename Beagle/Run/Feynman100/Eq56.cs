using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq56 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var eps = Rnd.Random.NextSingle() * 2 + 1;
	    var p = Rnd.Random.NextSingle() * 2 + 1;
	    var z = Rnd.Random.NextSingle() * 2 + 1;
	    var r = Rnd.Random.NextSingle() * 2 + 1;
	    var x = Rnd.Random.NextSingle() * 2 + 1;
	    var y = Rnd.Random.NextSingle() * 2 + 1;
        
        inputs[0] = eps;
        inputs[1] = p;
        inputs[2] = z;
	    inputs[3] = r;
	    inputs[4] = x;
	    inputs[5] = y;

	    var term1 = 3f / (4f * MathF.PI * eps);
	    var term2 = p * z / MathF.Pow(r, 5);
	    var term3 = MathF.Sqrt(x*x + y*y);
	    var result = term1 * term2 * term3;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "p", "z", "r", "x", "y"];
    }
    #endregion
}
