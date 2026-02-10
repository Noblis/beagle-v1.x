using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq69 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var u = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var b = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var theta = Rnd.Random.NextSingle() * 4 + 1; // 1..5 rad

        inputs[0] = u; 
        inputs[1] = b; 
        inputs[2] = theta;

        var result = -u * b * MathF.Cos(theta);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["u", "B", "Theta"];
    }
    #endregion
}
