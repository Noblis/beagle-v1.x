using BeagleLib.Agent;
using BeagleLib.Engine;
using BeagleLib.Engine.FitFunc;
using System.Runtime.CompilerServices;
using BeagleLib.Util;

namespace BeagleLib.VM;

public static class OrganismExt
{
    #region Comparision Methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMoreAccurateOrSameAccuracyButShorterThanMe(this Organism? me, Organism other)
    {
        return me == null || 
               me.Score < other.Score || 
               me.Score <= other.Score && me.Commands.Length > other.Commands.Length;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static bool IsSatisfactoryAccuracyAndShorterOrSameLengthButMoreAccurateThanMe(this Organism? me, Organism other, double solutionFoundASRThreshold)
    //{
    //    return other.ASR >= solutionFoundASRThreshold && 
    //           (me == null ||
    //            me.ASR < other.ASR && me.Commands.Length >= other.Commands.Length || //ASR is rounded and it should also be better
    //            me.ASR <= other.ASR && me.Commands.Length > other.Commands.Length);
    //}

    public static void UpdateScoreByAveraging<TFitFunc>(this Organism me, float[][] inputsArray, float[] correctOutputs, TFitFunc fitFunc) where TFitFunc : struct, IFitFunc
    {
        int score = 0;

        //We want to call this method in multi-threaded way, so we don't want nested multi-threading
        //Parallel.For(0, MLSetup.Current.ExperimentsPerGeneration, experiment =>
        //{
        //    var codeMachineCPU = new CodeMachine();
        //    _myOutputs[experiment] = codeMachineCPU.RunCommands(inputsArray[experiment], me.Commands);
        //});
        for (var experiment = 0; experiment < MLSetup.Current.ExperimentsPerGeneration; experiment++)
        {
            var codeMachineCPU = new CodeMachine();
            _myOutputs[experiment] = codeMachineCPU.RunCommands(inputsArray[experiment], me.Commands);
        }

        if (fitFunc.UseCorrelationFit)
        {
            //calculate output mean
            double outputsMean0 = 0;
            double outputsMean1 = 0;
            int count0 = 0;
            int count1 = 0;
            for (var experiment = 0; experiment < MLSetup.Current.ExperimentsPerGeneration; experiment++)
            {
                var output = _myOutputs[experiment];
                var correctOutput = correctOutputs[experiment];

                //valid/invalid outputs
                var isOutputValid = output.IsValidNumber();
                var isCorrectOutputValid = correctOutput.IsValidNumber();

                if (isOutputValid && isCorrectOutputValid)
                {
                    count0++;
                    outputsMean0 += output;
                    outputsMean1 += correctOutput;
                }
            }
            if (count0 != 0)
            {
                outputsMean0 /= count0;
                outputsMean1 /= count0;
            }
            count0 = 0;

            //calculate score
            var sums = new double[3];
            for (var experiment = 0; experiment < MLSetup.Current.ExperimentsPerGeneration; experiment++)
            {
                var output = _myOutputs[experiment];
                var correctOutput = correctOutputs[experiment];

                //valid/invalid outputs
                var isOutputValid = output.IsValidNumber();
                var isCorrectOutputValid = correctOutput.IsValidNumber();

                if (isOutputValid && isCorrectOutputValid)
                {
                    //if both output and correct output are valid
                    var outputDeltaVsMean = output - outputsMean0;
                    var correctOutputDeltaVsMean = correctOutput - outputsMean1;
                    sums[0] += outputDeltaVsMean * correctOutputDeltaVsMean;
                    sums[1] += outputDeltaVsMean * outputDeltaVsMean;
                    sums[2] += correctOutputDeltaVsMean * correctOutputDeltaVsMean;
                }
                else
                {
                    //if at least one of the outputs is invalid we end up here, XOR returns true if values are different
                    if (isOutputValid ^ isCorrectOutputValid) count0++; //if they are different
                    else count1++; //if they are the same
                }
            }

            if (sums[0].IsValidNumber() && sums[1].IsValidNumber() && sums[2].IsValidNumber())
            {
                var denominator = sums[1] * sums[2];
                float rSquared = 0;
                if (denominator != 0) rSquared = (float)(sums[0] * sums[0] / denominator);

                //r can range from 0 to 1
                //punishment is based on the percentage of mismatches, number of experiments cancels out
                score = (int)(BConfig.MaxScore * (MLSetup.Current.ExperimentsPerGeneration - count0 - count1) * rSquared) - BConfig.MaxScore * (count0 - count1);
            }
            else
            {
                score = (int)(-BConfig.MaxScore * MLSetup.Current.ExperimentsPerGeneration);
            }
        }
        else
        {
            for (var experiment = 0; experiment < MLSetup.Current.ExperimentsPerGeneration; experiment++)
            {
                var output = _myOutputs[experiment];
                var correctOutput = correctOutputs[experiment];

                //valid/invalid outputs
                var isOutputValid = output.IsValidNumber();
                var isCorrectOutputValid = correctOutput.IsValidNumber();

                if (isOutputValid && isCorrectOutputValid) Interlocked.Add(ref score, fitFunc.FitFunction(output, correctOutput));
                else Interlocked.Add(ref score, fitFunc.FitFunctionIfInvalid(isOutputValid, isCorrectOutputValid));
            }
        }

        me.Score = (me.Score + score) / 2;
    }
    private static readonly float[] _myOutputs = new float[MLSetup.Current.ExperimentsPerGeneration];
    #endregion
}