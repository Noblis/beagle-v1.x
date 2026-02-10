using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq72 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var omega = Rnd.Random.NextSingle() * 2 + 2;
        var c = Rnd.Random.NextSingle() * 2 + 4;
        var d = Rnd.Random.NextSingle() * 1 + 1;

        inputs[0] = omega;
        inputs[1] = c;
        inputs[2] = d;

        var denom = c * c - MathF.PI * MathF.PI/(d * d);
        var result = MathF.Sqrt(omega*omega / denom );
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Omega", "c", "d"];
    }
    #endregion
}
