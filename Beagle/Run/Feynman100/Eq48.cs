using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq48 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
	    var t = 1 + Rnd.Random.NextSingle() * 4;
	    var v2 = 1 + Rnd.Random.NextSingle() * 4;
        var v1 = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = n;
	    allInputs[inputSetIdx, 1] = k;
        allInputs[inputSetIdx, 2] = t;
        allInputs[inputSetIdx, 3] = v2;
        allInputs[inputSetIdx, 4] = v1;
        
        var result = n*k*t*MathF.Log(v2/v1);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","k","T","v2","v1"];
    }
    #endregion
}
