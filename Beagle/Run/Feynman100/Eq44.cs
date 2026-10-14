using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq44 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var h = 1 + Rnd.Random.NextSingle() * 4;
	    var w = 1 + Rnd.Random.NextSingle() * 4;
	    var c = 1 + Rnd.Random.NextSingle() * 4;
	    var k = 1 + Rnd.Random.NextSingle() * 4;
        var t = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = h;
        allInputs[inputSetIdx, 1] = w;
	    allInputs[inputSetIdx, 2] = c;
	    allInputs[inputSetIdx, 3] = k;
	    allInputs[inputSetIdx, 4] = t;

        var result = (h*w*w*w)/(MathF.PI*MathF.PI*c*c*(MathF.Exp((h*w)/(k*t))-1));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["h","w","c","k","T"];
    }
    #endregion
}
