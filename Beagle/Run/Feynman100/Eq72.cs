using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq72 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var omega = Rnd.Random.NextSingle() * 2 + 2;
        var c = Rnd.Random.NextSingle() * 2 + 4;
        var d = Rnd.Random.NextSingle() * 1 + 1;

        allInputs[inputSetIdx, 0] = omega;
        allInputs[inputSetIdx, 1] = c;
        allInputs[inputSetIdx, 2] = d;

        var denom = c * c - MathF.PI * MathF.PI/(d * d);
        var result = MathF.Sqrt(omega*omega / denom );
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Omega", "c", "d"];
    }
    #endregion
}
