using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq98 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var beta = 1 + Rnd.Random.NextSingle() * 4;   
        var alpha = 1 + Rnd.Random.NextSingle() * 4;  
        var theta = 1 + Rnd.Random.NextSingle() * 4;  

        inputs[0] = beta;
        inputs[1] = alpha;
        inputs[2] = theta;

        var result = beta * (1f + alpha * MathF.Cos(theta));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["Beta", "Alpha", "Theta"];
    }
    #endregion
}
