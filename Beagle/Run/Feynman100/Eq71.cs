using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq71 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var q = Rnd.Random.NextSingle() * 4 + 1; 
        var eps = Rnd.Random.NextSingle() * 4 + 1; 
        var r = Rnd.Random.NextSingle() * 4 + 1;  
        var v = Rnd.Random.NextSingle() * 1 + 1;  
        var c = Rnd.Random.NextSingle() * 7 + 3;

        allInputs[inputSetIdx, 0] = q;
        allInputs[inputSetIdx, 1] = eps;
        allInputs[inputSetIdx, 2] = r;
        allInputs[inputSetIdx, 3] = v;
        allInputs[inputSetIdx, 4] = c;
        
        var result = q / (4f * MathF.PI * eps * r * (1f - v/c));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "Eps", "r", "v", "c"];
    }
    #endregion
};
