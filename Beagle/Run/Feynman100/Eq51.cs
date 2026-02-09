using BeagleLib.Util;

namespace Run.Feynman100;

public class Eq51 : FeynmanMLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x1 = Rnd.Random.NextSingle() * 2 + 1;
        var w = Rnd.Random.NextSingle() * 2 + 1;
        var t = Rnd.Random.NextSingle() * 2 + 1;
	    var a = Rnd.Random.NextSingle() * 2 + 1;

        inputs[0] = x1;
        inputs[1] = w;
        inputs[2] = t;
	    inputs[3] = a;

        var cos = MathF.Cos(w * t);
        var result = x1 * (cos + a * cos * cos);
        //var result = x1 *( MathF.Cos((w*t))+ a * MathF.Cos( MathF.Pow((w*t),2)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "w", "t", "a"];
    }
    #endregion
}
