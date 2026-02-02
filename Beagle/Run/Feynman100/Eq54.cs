using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_54 : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var e = Rnd.Random.NextSingle() * 4 + 1;
        var q = Rnd.Random.NextSingle() * 4 + 1;
        var r = Rnd.Random.NextSingle() * 4 + 1;

        inputs[0] = q;
        inputs[1] = e;
        inputs[2] = r;
    
        var result = (q)/ ( 4 * MathF.PI * e * r);
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["q", "e", "r"];
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
