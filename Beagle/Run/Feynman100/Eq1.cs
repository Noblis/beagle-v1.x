using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.Feynman100;

public class Eq1 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var theta = 1 + Rnd.Random.NextSingle() * 2;

        inputs[0] = theta;

        // Checked
        var result = MathF.Exp(-theta * theta / 2f) / MathF.Sqrt(2f * MathF.PI);
        //var result = MathF.Exp(-MathF.Pow(x1, 2) / 2f) / MathF.Sqrt(2f * MathF.PI);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Theta"];
    }
    #endregion
}