using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq49 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var gamma = 1 + Rnd.Random.NextSingle() * 4;
	    var pr = 1 + Rnd.Random.NextSingle() * 4;
        var p = 1 + Rnd.Random.NextSingle() * 4;

        allInputs[inputSetIdx, 0] = gamma;
	    allInputs[inputSetIdx, 1] = pr;
	    allInputs[inputSetIdx, 2] = p;

        var result = MathF.Sqrt(gamma*pr/p);
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["gamma","pr","p"];
    }
    #endregion
}
