using BeagleLib.Util;

namespace Run.Feynman100;
public class Eq64 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 1 + 0; // 0..1
        var alpha = Rnd.Random.NextSingle() * 1 + 0; // 0..1
        var eps = Rnd.Random.NextSingle() * 1 + 1; // 1.2
        var e = Rnd.Random.NextSingle() * 1 + 1; // 1..2

        inputs[0] = n; 
        inputs[1] = alpha; 
        inputs[2] = eps; 
        inputs[3] = e;

        // f = (n*alpha / (1 - n*alpha/3)) * e * E
        var result = n * alpha / (1f - n*alpha/3f) * eps * e;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "Alpha", "e", "E"];
    }
    #endregion
}
