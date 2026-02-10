using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq70 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var p = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var e = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var theta = Rnd.Random.NextSingle() * 4 + 1; // 1..5 rad

        inputs[0] = p; 
        inputs[1] = e; 
        inputs[2] = theta;

        // f = -p * E * Cos(theta)
        var result = -p * e * MathF.Cos(theta);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "E", "Theta"];
    }
    #endregion
}
