using ILGPU;

namespace BeagleLib.Engine.FitFunc;

public struct HardcodedCorrelationFit : IFitFunc
{
    public bool UseHardcodedCorrelationFit => true;

    public int FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput)
    {
        throw new InvalidOperationException();
    }

    public int FitFunction(float output, float correctOutput)
    {
        throw new InvalidOperationException();
    }

    public int FitFunctionIfInvalid(bool isOutputValid, bool isCorrectOutputValid)
    {
        throw new InvalidOperationException();
    }
}