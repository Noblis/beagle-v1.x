using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq7 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = 1 + Rnd.Random.NextSingle() * 4;
        var x2 = 1 + Rnd.Random.NextSingle() * 4;
        var x3 = 1 + Rnd.Random.NextSingle() * 4;
        var y1 = 1 + Rnd.Random.NextSingle() * 4;
        var y2 = 1 + Rnd.Random.NextSingle() * 4;
        var y3 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = x1;
        inputs[1] = x2;
        inputs[2] = x3;
        inputs[3] = y1;
        inputs[4] = y2;
        inputs[5] = y3;

        var result = x1*y1 + x2*y2 + x3*y3;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "x2", "x3", "y1", "y2", "y3"];
    }
    #endregion
}