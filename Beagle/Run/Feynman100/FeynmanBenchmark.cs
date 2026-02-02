using BeagleLib.Engine;
using BeagleLib.Engine.FitFunc;

namespace Run.Feynman100;

public static class FeynmanBenchmark
{
    public static MLEngineCore GetFeynmanMLEngineForFormula(int formulaId)
    {
        return new MLEngine<Eq1, CorrelationFitFunc>(forceCPUAccelerator: false);
    }
}