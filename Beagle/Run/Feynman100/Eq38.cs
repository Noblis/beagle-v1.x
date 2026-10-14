using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq38 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var l1 = 1 + Rnd.Random.NextSingle() * 4;
	    var l2 = 1 + Rnd.Random.NextSingle() * 4;
	    var sigma = 1 + Rnd.Random.NextSingle() * 4;
	    
	    allInputs[inputSetIdx, 0] = l1;
	    allInputs[inputSetIdx, 1] = l2;
	    allInputs[inputSetIdx, 2] = sigma;
	    
        var result = l1 + l2 + 2f*MathF.Sqrt(l1*l2)*MathF.Cos(sigma);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["l1","l2","sigma"];
    }
    #endregion
}
