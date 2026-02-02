using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.Feynman100;
public class eq_53 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var p = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = p;
        inputs[1] = r;
    
        var result = (p)/ ( 4 * MathF.PI * MathF.Pow(r, 2));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["p", "r"];
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
