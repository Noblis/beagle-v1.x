using System.Diagnostics;
using ILGPU;

namespace BeagleLib.Engine.FitFunc;

public struct CorrelationFitFunc : IFitFunc
{
    public bool UseHardcodedCorrelationFit => true;

    public int FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput)
    {
        Debug.Assert(false, "CorrelationFitFunc.FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput) call");
        return 0;
    }

    public int FitFunction(float output, float correctOutput)
    {
        Debug.Assert(false, "CorrelationFitFunc.FitFunction (float output, float correctOutput) call");
        return 0;
    }

    public int FitFunctionIfInvalid(bool isOutputValid, bool isCorrectOutputValid)
    {
        Debug.Assert(false, "CorrelationFitFunc.FitFunction(bool isOutputValid, bool isCorrectOutputValid) call");
        return 0;
    }
}