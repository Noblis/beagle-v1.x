using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class DemoForJdAndTheBoys : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var a = Rnd.Random.NextSingle() * 10 - 5;
        var b = Rnd.Random.NextSingle() * 10 - 5;
        inputs[0] = a;
        inputs[1] = b;
        var output = 3 * a * a * MathF.Sin(b) + 1 / (a * a);
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b"];
    }
    public override int TargetColonySize(int generation)
    {
        if (generation == 0) return 100_000_000;
        //if (generation <= 10) return 100_000_000;
        return 10_000_000;
    }
    #endregion
}