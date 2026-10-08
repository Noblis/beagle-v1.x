using System.Diagnostics;
using BeagleLib.Engine.FitFunc;
using BeagleLib.Util;
using BeagleLib.VM;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;

namespace BeagleLib.Engine;

public static class MainKernel
{
    #region Kernel
    public static void Kernel<TFitFunc>(
        uint numberOfExperiments,
        ArrayView<int> scriptStarts,
        ArrayView<Command> allCommands,
        uint groupStart,
        ArrayView<float> allInputs,
        uint inputsCount,
        ArrayView<float> correctOutputs,

        ArrayView<int> rewards,
        TFitFunc fitFunc)

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
        var output = new CodeMachine().RunCommands(inputs, commands);

        //get correct output
        var correctOutput = correctOutputs[groupStart + experimentIdx];

        //valid/invalid outputs
        var isOutputValid = output.IsValidNumber();
        var isCorrectOutputValid = correctOutput.IsValidNumber();

        if (fitFunc.UseCorrelationFit)
        {
            //Patch D: replace the shared-memory atomic + barrier dance with hierarchical group
            //reductions. Per-thread partials -> warp/group reductions -> single score write per organism.
            double sumOut = 0, sumCorrect = 0;
            var countVV = 0;
            if (isOutputValid && isCorrectOutputValid)
            {
                countVV = 1;
                sumOut = output;
                sumCorrect = correctOutput;
            }

            var countTotal = GroupExtensions.AllReduce<int, AddInt32>(countVV);
            var sumOutTotal = GroupExtensions.AllReduce<double, AddDouble>(sumOut);
            var sumCorrectTotal = GroupExtensions.AllReduce<double, AddDouble>(sumCorrect);

            //means: identical in every thread of the group
            var meanOut = countTotal != 0 ? sumOutTotal / countTotal : 0;
            var meanCorrect = countTotal != 0 ? sumCorrectTotal / countTotal : 0;

            double sxy = 0, sxx = 0, syy = 0;
            var mismatches = 0;
            var invMatches = 0;
            if (isOutputValid && isCorrectOutputValid)
            {
                var outputDeltaVsMean = output - meanOut;
                var correctOutputDeltaVsMean = correctOutput - meanCorrect;
                sxy = outputDeltaVsMean * correctOutputDeltaVsMean;
                sxx = outputDeltaVsMean * outputDeltaVsMean;
                syy = correctOutputDeltaVsMean * correctOutputDeltaVsMean;
            }
            else
            {
                //XOR returns true if values are different
                if (isOutputValid ^ isCorrectOutputValid) mismatches = 1;
                else invMatches = 1;
            }

            var sumXYTotal = GroupExtensions.AllReduce<double, AddDouble>(sxy);
            var sumXXTotal = GroupExtensions.AllReduce<double, AddDouble>(sxx);
            var sumYYTotal = GroupExtensions.AllReduce<double, AddDouble>(syy);
            var mismatchTotal = GroupExtensions.AllReduce<int, AddInt32>(mismatches);
            var invMatchTotal = GroupExtensions.AllReduce<int, AddInt32>(invMatches);

            if (Group.IsFirstThread)
            {
                int score;
                if (sumXYTotal.IsValidNumber() && sumXXTotal.IsValidNumber() && sumYYTotal.IsValidNumber())
                {
                    var denominator = sumXXTotal * sumYYTotal;
                    float rSquared = 0;
                    if (denominator != 0) rSquared = (float)(sumXYTotal * sumXYTotal / denominator);

                    Debug.Assert(rSquared is <= 1 and >= 0);

                    //r can range from 0 to 1
                    //punishment is based on the percentage of mismatches, number of experiments cancels out
                    score = (int)(BConfig.MaxScore * (numberOfExperiments - (mismatchTotal + invMatchTotal)) * rSquared * rSquared) - BConfig.MaxScore * (mismatchTotal - invMatchTotal);
                }
                else
                {
                    score = (int)(-BConfig.MaxScore * numberOfExperiments);
                }
                rewards[organismIdx] = score;
            }
        }
        else
        {
            int score;
            //if (isOutputValid && isCorrectOutputValid) score = fitFunc.FitFunction(allInputs, (uint)(groupStart + experimentIdx * inputsCount), inputsCount, output, correctOutput);
            if (isOutputValid && isCorrectOutputValid) score = fitFunc.FitFunction(output, correctOutput);
            else score = fitFunc.FitFunctionIfInvalid(isOutputValid, isCorrectOutputValid);

            //Patch B: one block = one organism (Group.DimX == numberOfExperiments), so
            //replace global atomics with a block reduction: one global write per organism.
            var total = GroupExtensions.AllReduce<int, AddInt32>(score);
            if (Group.IsFirstThread) rewards[organismIdx] = total;
        }
    }
    #endregion
}