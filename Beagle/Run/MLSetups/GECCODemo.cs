using BeagleLib.Engine;
using BeagleLib.VM;

namespace Run.MLSetups;

public class GECCODemo : MLSetup
{
    public override string[] GetInputLabels()
    {
        return ["a", "b"];
    }

    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputsToFill)
    {
        var a = Random.Shared.NextSingle() * 100;
        var b = Random.Shared.NextSingle() * 100;

        inputsToFill[0] = a;
        inputsToFill[1] = b;
        
        var result = (3*a - b*b) / MathF.PI;

        return (inputsToFill, result);
    }

    public override bool KeepOptimizingAfterSolutionFound => true;

    public override OpEnum[] GetAllowedOperations() => base.GetAllowedOperations()
        .Where(x => x != OpEnum.Arccos && 
                    x != OpEnum.Arcsin && 
                    x != OpEnum.Arctan && 
                    x != OpEnum.Cos && 
                    x != OpEnum.Sin && 
                    x != OpEnum.Tan && 
                    x != OpEnum.Tanh)
        .ToArray();

    //public override int TargetColonySize(int generation) => 250_000;

    //public override double SolutionFoundASRThreshold => 0.97;
}