using BeagleLib.Engine.FitFunc;
using BeagleLib.Util;
using BeagleLib.VM;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using System.Diagnostics;

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
        //var index = Group.IdxX + (long)Grid.IdxX * Group.DimX; //TODO: Grid.LongGlobalIndex.X;
        //var organismIdx = index / numberOfExperiments;
        //var experimentIdx = index % numberOfExperiments;
        var organismIdx = Grid.IdxX;
        var experimentIdx = Group.IdxX;

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
            bool bothValid = isOutputValid && isCorrectOutputValid;
            bool bothInvalid = !isOutputValid && !isCorrectOutputValid;

            // Phase 1: calculate the means.
            int validCount = GroupExtensions.Reduce<int, AddInt32>(bothValid ? 1 : 0);
            double sumOutput = GroupExtensions.Reduce<double, AddDouble>(bothValid ? (double)output : 0.0);
            double sumCorrect = GroupExtensions.Reduce<double, AddDouble>(bothValid ? (double)correctOutput : 0.0);

            // Only the first thread needs to publish the means.
            var means = SharedMemory.Allocate<double>(2);

            if (Group.IsFirstThread)
            {
                means[0] = validCount > 0 ? sumOutput / validCount : 0.0;
                means[1] = validCount > 0 ? sumCorrect / validCount : 0.0;
            }

            Group.Barrier();

            // Phase 2: calculate centered sums.
            double dx = bothValid ? output - means[0] : 0.0;
            double dy = bothValid ? correctOutput - means[1] : 0.0;
            double sxy = GroupExtensions.Reduce<double, AddDouble>(dx * dy);
            double sxx = GroupExtensions.Reduce<double, AddDouble>(dx * dx);
            double syy = GroupExtensions.Reduce<double, AddDouble>(dy * dy);

            // No need to reduce the mismatch count separately.
            int invalidMatchCount = GroupExtensions.Reduce<int, AddInt32>(bothInvalid ? 1 : 0);

            // Only thread zero needs the totals.
            if (Group.IsFirstThread)
            {
                int score;

                if (sxy.IsValidNumber() && sxx.IsValidNumber() && syy.IsValidNumber())
                {
                    double denominator = sxx * syy;
                    float rSquared = 0f;
                    if (denominator != 0.0) rSquared = (float)(sxy * sxy / denominator);
                    int mismatches = (int)numberOfExperiments - validCount - invalidMatchCount;
                    score = (int)(BConfig.MaxScore * validCount * rSquared * rSquared) - BConfig.MaxScore * (mismatches - invalidMatchCount);
                }
                else
                {
                    score = (int)(-BConfig.MaxScore * numberOfExperiments);
                }

                rewards[organismIdx] = score;
            }

            ////we first use this variable to calculate total, then divide by count to get the Mean
            ////this is one element array because we cannot allocate scalar values in shared memory
            ////0 is output, 1 is correct output mean
            //var outputsMean = SharedMemory.Allocate<double>(2);

            ////we first use this variable to keep the count of valid outputs, then to keep the count of misaligned invalid outputs
            ////this is one element array because we cannot allocate scalar values in shared memory

            ////we use the second count to count the number of invalid/invalid matches
            //var count = SharedMemory.Allocate<int>(2);

            ////allocate three sums, to be used later
            //var sums = SharedMemory.Allocate<double>(3);

            ////set all to zero
            //if (Group.IsFirstThread)
            //{
            //    outputsMean[0] = 0;
            //    outputsMean[1] = 0;

            //    count[0] = 0;
            //    count[1] = 0; //set count1 to zero, we count invalid/invalid matches

            //    sums[0] = 0;
            //    sums[1] = 0;
            //    sums[2] = 0;
            //}
            //Group.Barrier();

            //if (isOutputValid && isCorrectOutputValid)
            //{
            //    Atomic.Add(ref count[0], 1);
            //    Atomic.Add(ref outputsMean[0], output);
            //    Atomic.Add(ref outputsMean[1], correctOutput);
            //}
            //Group.Barrier();

            ////using first thread, divide the sum over the count ot get the average, reset count, allocate sums and init them to zero
            //if (Group.IsFirstThread)
            //{
            //    if (count[0] != 0)
            //    {
            //        outputsMean[0] /= count[0];
            //        outputsMean[1] /= count[0];
            //    }

            //    count[0] = 0; //reset count0 to now be used to count the number of valid/invalid & invalid/valid mismatches
            //}
            //Group.Barrier();

            ////accumulate three sums across all threads in the block
            //if (isOutputValid && isCorrectOutputValid)
            //{
            //    //if both output and correct output are valid
            //    var outputDeltaVsMean = output - outputsMean[0];
            //    var correctOutputDeltaVsMean = correctOutput - outputsMean[1];
            //    Atomic.Add(ref sums[0], outputDeltaVsMean * correctOutputDeltaVsMean);
            //    Atomic.Add(ref sums[1], outputDeltaVsMean * outputDeltaVsMean);
            //    Atomic.Add(ref sums[2], correctOutputDeltaVsMean * correctOutputDeltaVsMean);
            //}
            //else
            //{
            //    //if at least one of the outputs is invalid we end up here, XOR returns true if values are different
            //    if (isOutputValid ^ isCorrectOutputValid) Atomic.Add(ref count[0], 1); //if they are different
            //    else Atomic.Add(ref count[1], 1); //if they are the same
            //}
            //Group.Barrier();

            ////store r squared results for returning data from the Kernel
            //if (Group.IsFirstThread)
            //{
            //    int score;
            //    if (sums[0].IsValidNumber() && sums[1].IsValidNumber() && sums[2].IsValidNumber())
            //    {
            //        var denominator = sums[1] * sums[2];
            //        float rSquared = 0;
            //        if (denominator != 0) rSquared = (float)(sums[0] * sums[0] / denominator);

            //        Debug.Assert(rSquared is <= 1 and >= 0);

            //        //r can range from 0 to 1
            //        //punishment is based on the percentage of mismatches, number of experiments cancels out
            //        score = (int)(BConfig.MaxScore * (numberOfExperiments - (count[0] + count[1])) * rSquared * rSquared) - BConfig.MaxScore * (count[0] - count[1]);
            //    }
            //    else
            //    {
            //        score = (int)(-BConfig.MaxScore * numberOfExperiments);
            //    }
            //    rewards[organismIdx] = score;
            //}
        }
        else
        {
            int score;
            //if (isOutputValid && isCorrectOutputValid) score = fitFunc.FitFunction(allInputs, (uint)(groupStart + experimentIdx * inputsCount), inputsCount, output, correctOutput);
            if (isOutputValid && isCorrectOutputValid) score = fitFunc.FitFunction(output, correctOutput);
            else score = fitFunc.FitFunctionIfInvalid(isOutputValid, isCorrectOutputValid);

            //accumulate results
            //Atomic.Add(ref rewards[organismIdx], score);

            var total = GroupExtensions.Reduce<int, AddInt32>(score);
            if (Group.IsFirstThread) rewards[organismIdx] = total;
        }
    }
    #endregion
}