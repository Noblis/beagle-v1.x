using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class KatyaBall : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = Rnd.Random.NextSingle()*6;
        var x2 = Rnd.Random.NextSingle() * 6;
        var x3 = Rnd.Random.NextSingle() * 6;
        var x4 = Rnd.Random.NextSingle() * 6;
        var x5 = Rnd.Random.NextSingle() * 6;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;
        inputs[3] = x4;
        inputs[4] = x5;

        var output = 10f/(MathF.Pow(x1-3f,2f)+MathF.Pow(x2-3f,2f)+MathF.Pow(x3-3f,2f)+MathF.Pow(x4-3f,2f)+MathF.Pow(x5-3f,2f)+5f);
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["x1","x2","x3","x4","x5"];
    }

    public override double SolutionFoundASRThreshold => 1.0;
    public override long TotalBirthsToResetColonyIfNoProgress => 120_000_000;
    public override bool KeepOptimizingAfterSolutionFound => true;
    public override double CrossoverRate => 0.20;
    public override double CrossoverPartnerDelta => .9;

    public override int TargetColonySize(int generation)
    {
        //if (generation % 1000 < 5) return 5_000_000;
        //if (generation % 1000 < 10) return 3_000_000;
        //if (generation % 1000 < 15) return 1_000_000;
        //if (generation % 1000 < 20) return 500_000;
        //if (generation % 1000 < 25) return 250_000;
        return 100_000;
    }

    #endregion
}