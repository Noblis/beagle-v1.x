using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq52 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var k = Rnd.Random.NextSingle() * 4 + 1;
        var t2 = Rnd.Random.NextSingle() * 4 + 1;
        var t1 = Rnd.Random.NextSingle() * 4 + 1;
        var a = Rnd.Random.NextSingle() * 4 + 1;
	    var d = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = k;
        inputs[1] = t2;
        inputs[2] = t1;
        inputs[3] = a;
	    inputs[4] = d;

	var result = k * (t2 - t1) * a / d;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["k", "T2","T1", "A", "d"];
    }
    #endregion
}
