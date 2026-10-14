using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq26 : FeynmanMLSetup
{
    #region Overrides
    public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var n = 0 + Rnd.Random.NextSingle() * 1;
	    var theta = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = n;
	    allInputs[inputSetIdx, 1] = theta;

        var result = MathF.Asin(n * MathF.Sin(theta));
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","Theta"];
    }
    #endregion
}
