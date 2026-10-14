using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq30 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var l = 1 + Rnd.Random.NextSingle() * 4;
	    var n = 1 + Rnd.Random.NextSingle() * 4;
	    var theta = 1 + Rnd.Random.NextSingle() * 4;
	    
	    allInputs[inputSetIdx, 0] = l;
	    allInputs[inputSetIdx, 1] = n;
	    allInputs[inputSetIdx, 2] = theta;

        var numerator = MathF.Sin(n * theta / 2);
        var denominator = MathF.Sin(theta / 2);
        var result = l * numerator * numerator / (denominator * denominator);
        
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["L","n","Theta"];
    }
    #endregion
}
