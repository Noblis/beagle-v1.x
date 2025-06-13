﻿using ILGPU;

namespace BeagleLib.Engine.FitFunc;

public interface IFitFunc
{
    int FitFunction(ArrayView<float> arrayViewInputs, uint startIdx, uint length, float output, float correctOutput);
    int FitFunction(float output, float correctOutput);
    int FitFunctionIfInvalid(bool isOutputValid, bool isCorrectOutputValid);
}