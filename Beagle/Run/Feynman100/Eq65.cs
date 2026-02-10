using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq65 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 1 + 0; // 0..1
        var alpha = Rnd.Random.NextSingle() * 1 + 0; // 1..3

        inputs[0] = n; 
        inputs[1] = alpha;

        var result = 1 + n * alpha / (1f - n * alpha/3f);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "Alpha"];
    }
    #endregion
}
