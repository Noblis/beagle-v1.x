
using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_52 : MLSetup
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
	var result = ( k * (t2 - t1) * a) / d;
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["k", "t2","t1", "a", "d"];
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
