using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq27 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var d1 = 1 + Rnd.Random.NextSingle() * 4;
        var d2 = 1 + Rnd.Random.NextSingle() * 4;
        var n = 1 + Rnd.Random.NextSingle() * 4;

        inputs[0] = d1;
        inputs[1] = d2;
	    inputs[2] = n;

        var result = 1/(1/d1+n/d2);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["n","d1","d2"];
    }
    #endregion
}
