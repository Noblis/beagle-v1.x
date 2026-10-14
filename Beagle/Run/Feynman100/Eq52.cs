using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq52 : FeynmanMLSetup
{
    #region Overrides
        public override (float[,], float) GetNextInputsAndCorrectOutput(float[,] allInputs, ushort inputSetIdx)
    {
        var k = Rnd.Random.NextSingle() * 4 + 1;
        var t2 = Rnd.Random.NextSingle() * 4 + 1;
        var t1 = Rnd.Random.NextSingle() * 4 + 1;
        var a = Rnd.Random.NextSingle() * 4 + 1;
	    var d = Rnd.Random.NextSingle() * 4 + 1;

        allInputs[inputSetIdx, 0] = k;
        allInputs[inputSetIdx, 1] = t2;
        allInputs[inputSetIdx, 2] = t1;
        allInputs[inputSetIdx, 3] = a;
	    allInputs[inputSetIdx, 4] = d;

	var result = k * (t2 - t1) * a / d;
        return (allInputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["k", "T2","T1", "A", "d"];
    }
    #endregion
}
