using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq79 : FeynmanMLSetup
{
    #region Overrides

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var g = 1 + Rnd.Random.NextSingle() * 4;
        var u = 1 + Rnd.Random.NextSingle() * 4;
        var b = 1 + Rnd.Random.NextSingle() * 4;
        var j = 1 + Rnd.Random.NextSingle() * 4;
        var h = 1 + Rnd.Random.NextSingle() * 4;
       
        inputs[0] = g;
        inputs[1] = u;
        inputs[2] = b;
        inputs[3] = j;
        inputs[4] = h;

        var result = g * u * b * j / h;

        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["g", "u", "B", "J", "h"];
    }
    #endregion
}

