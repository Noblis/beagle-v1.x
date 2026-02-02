using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_63 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // ranges: {1,5} shown in screenshot
	var n = Rnd.Random.NextSingle() * 4 + 1;
	var p = Rnd.Random.NextSingle() * 4 + 1;	
        var e = Rnd.Random.NextSingle() * 4 + 1;
        var k = Rnd.Random.NextSingle() * 4 + 1;
        var t = Rnd.Random.NextSingle() * 4 + 1;
        inputs[0] = n;
        inputs[1] = p;
        inputs[2] = e;
	inputs[3] = k;
	inputs[4] = t;
        // f = (3/5) * ( q^2 / (4*pi*eps*d) )
        var result = (n * p * p * e) / (3 * k * t) ;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "n", "p", "e", "k", "t" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}
