using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class Eq4 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var y1 = 1 + Rnd.Random.NextSingle() * 4;
        var y2 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = y1;
        inputs[3] = y2;

        var result = MathF.Sqrt((x2 - x1)*(x2 - x1) + (y2 - y1)*(y2 - y1));
        //var result = MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(x4 - x3, 2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "y1", "y2"];
    }
    #endregion
}
