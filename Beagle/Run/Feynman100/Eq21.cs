using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq21 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var m1 = 1 + Rnd.Random.NextSingle() * 4;
        var m2 = 1 + Rnd.Random.NextSingle() * 4;
        var r1 = 1 + Rnd.Random.NextSingle() * 4;
        var r2 = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = m1;
        inputs[1] = m2;
        inputs[2] = r1;
        inputs[3] = r2;

        var result = (m1*r1 + m2*r2) / (m1 + m2);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["m1", "m2", "r1", "r2"];
    }
    #endregion
}