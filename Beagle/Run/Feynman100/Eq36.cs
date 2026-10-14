using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq36 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var v = 1 + Rnd.Random.NextSingle() * 1;
	    var c = 3 + Rnd.Random.NextSingle() * 7;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    
	    allInputs[inputSetIdx, 0] = v;
	    allInputs[inputSetIdx, 1] = c;
	    allInputs[inputSetIdx, 2] = w;
	    
        var result = (1f+v/c)/(MathF.Sqrt(1f-v*v/(c*c)))*w;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["v","c","w"];
    }
    #endregion
}
