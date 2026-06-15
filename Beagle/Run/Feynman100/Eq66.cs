using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq66 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var eps = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var c = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var l = Rnd.Random.NextSingle() * 4 + 1; // 1..5
        var r = Rnd.Random.NextSingle() * 4 + 1; // 1..5

        inputs[0]=eps; 
        inputs[1]=c; 
        inputs[2]=l; 
        inputs[3]=r;

        var result = 1f / (4f * MathF.PI * eps * c*c) * (2f*l / r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Eps", "c", "l", "r"];
    }
    #endregion
}
