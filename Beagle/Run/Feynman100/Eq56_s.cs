using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_56_s : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges shown: omega {2,4}, c {4,6}, d {1,2}
        var e = Rnd.Random.NextSingle() * 2 + 1;
	var p = Rnd.Random.NextSingle() * 2 + 1;
	var z = Rnd.Random.NextSingle() * 2 + 1;
	var r = Rnd.Random.NextSingle() * 2 + 1;
	var x = Rnd.Random.NextSingle() * 2 + 1;
	var y = Rnd.Random.NextSingle() * 2 + 1;
        inputs[0] = e;
        inputs[1] = p;
        inputs[2] = z;
	inputs[3] = r;
	inputs[4] = x;
	inputs[5] = y;
	var term_1 = 3f / ( 4f * MathF.PI * e);
	var term_2 = (p * z) / (MathF.Pow(r, 5));
	var term_3 = MathF.Sqrt(MathF.Pow(x,2) + MathF.Pow(y,2));
	var result = term_1 * term_2 * term_3;
        // f = sqrt( omega^2 / ( c^2 - pi^2 / d^2 ) )
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "e", "p", "z", "r", "x", "y" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
