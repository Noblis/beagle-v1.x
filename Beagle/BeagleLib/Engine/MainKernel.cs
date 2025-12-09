using BeagleLib.Engine.FitFunc;
using BeagleLib.VM;
using ILGPU;

namespace BeagleLib.Engine;

public static class MainKernel
{
    #region Kernel
    public static void Kernel<TFitFunc>(
        byte useLibDevice,
        uint numberOfExperiments,
        ArrayView<int> scriptStarts,
        ArrayView<Command> allCommands,
        uint groupStart,
        ArrayView<float> allInputs,
        uint inputsCount,
        ArrayView<float> correctOutputs,
        float correctOutputsMean,

        ArrayView<int> rewards,
        TFitFunc gpuMLSetup)

        where TFitFunc : struct, IFitFunc
    {
        //Figure out the indexes
        var index = Group.IdxX + (long)Grid.IdxX * Group.DimX; //TODO: Grid.LongGlobalIndex.X;
        var organismIdx = index / numberOfExperiments;
        var experimentIdx = index % numberOfExperiments;

        //script start
        var myScriptStart = checked((uint)scriptStarts[organismIdx]);

        //script end and length
        var myScriptEnd = organismIdx >= scriptStarts.Length - 1 ? (int)allCommands.Length : scriptStarts[organismIdx + 1];
        var myScriptLength = checked((uint)(myScriptEnd - myScriptStart));

        //execute commands
        var inputs = allInputs.SubView((uint)((groupStart + experimentIdx) * inputsCount), inputsCount);
        var commands = allCommands.SubView(myScriptStart, myScriptLength);
        var output = new CodeMachine().RunCommands(inputs, commands, useLibDevice != 0);

        //get correct output
        var correctOutput = correctOutputs[groupStart + experimentIdx];

        //fit function plus script length adjustment
        var isOutputValid = !float.IsNaN(output) && !float.IsInfinity(output) && !float.IsNegativeInfinity(output);
        var isCorrectOutputValid = !float.IsNaN(correctOutput) && !float.IsInfinity(correctOutput) && !float.IsNegativeInfinity(correctOutput);

        int score;
        if (isOutputValid && isCorrectOutputValid) score = gpuMLSetup.FitFunction(allInputs, (uint)(groupStart + experimentIdx*inputsCount), inputsCount, output, correctOutput);
        else score = gpuMLSetup.FitFunctionIfInvalid(isOutputValid, isCorrectOutputValid);

        //accumulate results
        Atomic.Add(ref rewards[organismIdx], score);
    }
    #endregion
}