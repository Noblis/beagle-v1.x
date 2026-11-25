using BeagleLib.Engine;
using BeagleLib.Util;
using BeagleLib.VM;

namespace Run.MLSetups;

public class SinApproximation : MLSetup
{
    #region Overrides
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        var x = Rnd.Random.NextSingle() * 2 * MathF.PI - MathF.PI;
        inputs[0] = x;
        var output = MathF.Sin(x);
        return (inputs, output);
    }
    public override string[] GetInputLabels()
    {
        return ["x"];
    }
    public override int TargetColonySize(int generation)
    {
        if (generation % 1000 < 50) return 25_000_000;
        return 300_000;
    }
    public override long TotalBirthsToResetColonyIfNoProgress => 5_000_000_000;
    public override uint ExperimentsPerGeneration => 512;

    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations().Where(x => x != OpEnum.Sin && 
                                                                                              x != OpEnum.Cos && 
                                                                                              x != OpEnum.Arcsin &&
                                                                                              x != OpEnum.Arccos &&
                                                                                              x != OpEnum.Arctan &&
                                                                                              x != OpEnum.Tan &&
                                                                                              x != OpEnum.Tanh).ToArray();
    #endregion
}