using BeagleLib.Engine;
using BeagleLib.Engine.FitFunc;

namespace Run.Feynman100;

public static class FeynmanBenchmark
{
    public static MLEngineCore GetFeynmanMLEngineForFormula(int formulaId)
    {
        switch (formulaId)
        {
            case 1: return new MLEngine<Eq1, CorrelationFitFunc>(forceCPUAccelerator: false);
            case 2: return new MLEngine<Eq2, CorrelationFitFunc>(forceCPUAccelerator: false);
            default: throw new Exception($"Invalid Feynman Equation number {formulaId}. Mist be 1-100.");
        }


    }
}