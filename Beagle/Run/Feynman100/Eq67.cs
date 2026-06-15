using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq67 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var p = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var v = Rnd.Random.NextSingle() * 1 + 1; // 0..1 (fraction of c)
        var c = Rnd.Random.NextSingle() * 7 + 3; // 3..10

        inputs[0] = p; 
        inputs[1] = v; 
        inputs[2] = c;

        var result = p / MathF.Sqrt(1f - v*v/(c*c));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "v", "c"];
    }
    #endregion
}
