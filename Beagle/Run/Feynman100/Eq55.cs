using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_55 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var e = Rnd.Random.NextSingle() * 2 + 1;
        var p = Rnd.Random.NextSingle() * 2 + 1;
        var t = Rnd.Random.NextSingle() * 2 + 1;
        var r = Rnd.Random.NextSingle() * 2 + 1;

        inputs[0] = e;
        inputs[1] = p;
        inputs[2] = t;
        inputs[3] = r;
    
        var result = (1 / ( 4 * MathF.PI * e)) * ((p * MathF.Cos(t))/ MathF.Pow(r, 2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return new[] {"e", "p", "t", "r"};
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 1_500_000_000;
    public override double SolutionFoundASRThreshold => 1.0;
    public override bool KeepOptimizingAfterSolutionFound => true;
    //public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Sin &&
    //                                                                                          x != OpEnum.Add &&
    //                                                                                          x != OpEnum.Sub &&
    //                                                                                          x != OpEnum.Cbrt &&
    //                                                                                          x != OpEnum.Cube &&
    //                                                                                          x != OpEnum.Ln).ToArray();
    #endregion
}
