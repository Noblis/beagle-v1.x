using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class Eq58S : MLSetupV16
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var d = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = eps;
        allInputs[inputSetIdx, 2] = d;

        // f = (3/5) * ( q^2 / (4*pi*eps*d) )
        var result = (3f / 5f) * ((q * q) / (4f * MathF.PI * eps * d));

        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "q", "Eps", "d" };
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}