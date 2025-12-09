using System.Diagnostics;
using ILGPU;

namespace BeagleLib.Engine.FitFunc;

public struct HardcodedCorrelationFit : IFitFunc
{
    public bool UseHardcodedCorrelationFit => true;

    public int FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput)
    {
        Debug.Assert(false, "HardcodedCorrelationFit.FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput) call");
        return 0;
    }

    public int FitFunction(float output, float correctOutput)
    {
        Debug.Assert(false, "HardcodedCorrelationFit.FitFunction (float output, float correctOutput) call");
        return 0;
    }

    public int FitFunctionIfInvalid(bool isOutputValid, bool isCorrectOutputValid)
    {
        Debug.Assert(false, "HardcodedCorrelationFit.FitFunction(bool isOutputValid, bool isCorrectOutputValid) call");
        return 0;
    }
}