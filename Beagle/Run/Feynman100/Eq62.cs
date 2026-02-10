using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq62 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        // random input generation (ranges shown in comments)
        var n = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var p = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var e = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var theta = Rnd.Random.NextSingle() * 2 + 1; // 1..3 (rad)
        var k = Rnd.Random.NextSingle() * 2 + 1; // 1..3
        var t = Rnd.Random.NextSingle() * 2 + 1; // 1..3

        inputs[0]=n; inputs[1]=p; inputs[2]=e; inputs[3]=theta; inputs[4]=k; inputs[5]=t;

        // f = n * (1 + (p * E * Cos[theta])/(k*T))
        var result = n * (1f + p * e * MathF.Cos(theta)/(k * t));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n", "p", "E", "Theta", "k", "T"];
    }
    #endregion
}
