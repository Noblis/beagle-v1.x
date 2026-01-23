using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class Eq58s : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var eps = Rnd.Random.NextSingle() * 4 + 1;
        var d = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = q;
        inputs[1] = eps;
        inputs[2] = d;

        // f = (3/5) * ( q^2 / (4*pi*eps*d) )
        var result = (3f / 5f) * ((q * q) / (4f * MathF.PI * eps * d));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] { "q", "eps", "d" };
    }
    //public override int TargetColonySize(int generation)
    //{
    //    if (generation % 1000 < 20) return 5_000_000;
    //    return 1_000_000;
    //}
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    //public override uint ExperimentsPerGeneration => 1024;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    #endregion
}