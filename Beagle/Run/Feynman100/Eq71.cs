using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq71 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1; 
        var eps = Rnd.Random.NextSingle() * 4 + 1; 
        var r = Rnd.Random.NextSingle() * 4 + 1;  
        var v = Rnd.Random.NextSingle() * 1 + 1;  
        var c = Rnd.Random.NextSingle() * 7 + 3; 

        inputs[0] = q;
        inputs[1] = eps;
        inputs[2] = r;
        inputs[3] = v;
        inputs[4] = c;
        
        var result = q / (4f * MathF.PI * eps * r * (1f - v/c));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Eps", "r", "v", "c"];
    }
    #endregion
};
