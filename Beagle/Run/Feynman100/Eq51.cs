using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;
namespace Run.MLSetups;
public class eq_51 : MLSetup
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
        var result = x1 *( MathF.Cos((w*t))+ a * MathF.Cos( MathF.Pow((w*t),2)));
        return (inputs, result);
    }
    public override string[] GetInputLabels()
    {
        return ["x1", "w", "t", "a"];
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
