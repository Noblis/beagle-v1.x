using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class AvgOf2 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var a = Rnd.Random.NextSingle() * 100;
        var b = Rnd.Random.NextSingle() * 100;
        inputs[0] = a;
        inputs[1] = b;
        return (inputs, (a + b) / 2);
    }
    public override string[] GetInputLabels()
    {
        return ["a", "b"];
    }

    public override int TargetColonySize(int generation)
    {
        if (generation == 0) return 500;
        
        return 1_000_000;
    }
    #endregion
}