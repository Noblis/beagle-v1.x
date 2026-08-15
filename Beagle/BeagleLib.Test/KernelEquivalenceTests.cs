using System;
using System.Collections.Generic;
using System.Linq;
using BeagleLib.Engine;
using BeagleLib.Engine.FitFunc;
using BeagleLib.Util;
using BeagleLib.VM;
using ILGPU;
using ILGPU.Runtime;
using NUnit.Framework;
using Command = BeagleLib.VM.Command;

namespace BeagleLib.Test;

//Verifies that the GPU scoring kernel produces exactly the same rewards as a CPU reference
//implementation. Guard rail for the kernel optimizations (block reductions, etc.).
public class KernelEquivalenceTests
{
    private const uint Experiments = 512;
    private const uint InputsCount = 3;

    private static Command[][] BuildOrganisms()
    {
        return new[]
        {
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Const, 1.5f), new Command(OpEnum.Add) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Dup), new Command(OpEnum.Mul), new Command(OpEnum.Const, 0.5f), new Command(OpEnum.Mul) },
            new[] { new Command(OpEnum.Const, 9.0f), new Command(OpEnum.Load, 0), new Command(OpEnum.Sqrt), new Command(OpEnum.Add) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Load, 1), new Command(OpEnum.Swap), new Command(OpEnum.Mul), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Add) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Sin), new Command(OpEnum.Const, 3.0f), new Command(OpEnum.Div) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Exp), new Command(OpEnum.Ln), new Command(OpEnum.Const, 1.0f), new Command(OpEnum.Sub) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Dup), new Command(OpEnum.Add), new Command(OpEnum.Const, 1.0f), new Command(OpEnum.Add) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Copy, 0), new Command(OpEnum.Paste, 0), new Command(OpEnum.Add) },
        };
    }

    [Test]
    public void StdScoringMatchesCPU()
    {
        var organisms = BuildOrganisms();
        var organismCount = organisms.Length;

        var cpuRewards = new int[organismCount];
        var inputs = new float[Experiments * InputsCount];
        var correctOutputs = new float[Experiments];
        var rnd = new Random(12345);
        for (var e = 0; e < Experiments; e++)
        {
            inputs[e * InputsCount] = (float)(rnd.NextDouble() * 4.0 - 2.0);
            correctOutputs[e] = (float)(rnd.NextDouble() * 4.0 - 2.0);
        }

        var codeMachine = new CodeMachine();
        var fit = new StdFitFunc();
        for (var o = 0; o < organismCount; o++)
        {
            var sum = 0;
            for (var e = 0; e < Experiments; e++)
            {
                var inputSlice = new float[InputsCount];
                Array.Copy(inputs, (int)(e * InputsCount), inputSlice, 0, (int)InputsCount);
                var output = codeMachine.RunCommands(inputSlice, organisms[o]);
                var isValid = output.IsValidNumber();
                var isCorrectValid = correctOutputs[e].IsValidNumber();
                sum += isValid && isCorrectValid
                    ? fit.FitFunction(output, correctOutputs[e])
                    : fit.FitFunctionIfInvalid(isValid, isCorrectValid);
            }
            cpuRewards[o] = sum;
        }

        using var context = Context.Create(builder => builder.Default().EnableAlgorithms());
        var device = context.Devices.FirstOrDefault(d => d.AcceleratorType == AcceleratorType.Cuda) ?? context.Devices.First();
        using var acc = device.CreateAccelerator(context);
        var kernel = acc.LoadKernel<uint, ArrayView<int>, ArrayView<Command>, uint, ArrayView<float>, uint, ArrayView<float>, ArrayView<int>, StdFitFunc>(MainKernel.Kernel);

        var scriptStarts = new int[organismCount];
        var allCommands = new List<Command>();
        for (var o = 0; o < organismCount; o++)
        {
            scriptStarts[o] = allCommands.Count;
            allCommands.AddRange(organisms[o]);
        }

        using var dScriptStarts = acc.Allocate1D(scriptStarts);
        using var dCommands = acc.Allocate1D(allCommands.ToArray());
        using var dInputs = acc.Allocate1D(inputs);
        using var dCorrect = acc.Allocate1D(correctOutputs);
        using var dRewards = acc.Allocate1D<int>(organismCount);
        var stream = acc.DefaultStream;

        dRewards.View.MemSetToZero(stream);
        kernel(stream, new KernelConfig(new Index1D(organismCount), new Index1D((int)Experiments)), Experiments,
               dScriptStarts.View, dCommands.View, 0u, dInputs.View, InputsCount, dCorrect.View, dRewards.View, new StdFitFunc());
        stream.Synchronize();
        var gpuRewards = new int[organismCount];
        dRewards.View.CopyToCPU(stream, gpuRewards);

        for (var o = 0; o < organismCount; o++)
        {
            if (gpuRewards[o] != cpuRewards[o])
            {
                TestContext.WriteLine($"organism {o}: GPU={gpuRewards[o]} CPU={cpuRewards[o]} commands=[{string.Join("; ", organisms[o])}]");
            }
            //GPU float reward math can differ from the CPU CLR by ±1 per experiment (FMA contraction etc.),
            //so allow up to Experiments/2 difference on the random-input catch-all test.
            Assert.That(Math.Abs(gpuRewards[o] - cpuRewards[o]), Is.LessThanOrEqualTo(Experiments / 2), $"organism {o} mismatch");
        }
    }

    //Scripts composed only of exact float ops (load/const/add/sub/mul/div-by-pow2/dup/swap/copy/paste).
    private static Command[][] ExactBuildOrganisms()
    {
        return new[]
        {
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Mul), new Command(OpEnum.Const, 1.0f), new Command(OpEnum.Add) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Load, 1), new Command(OpEnum.Mul), new Command(OpEnum.Load, 2), new Command(OpEnum.Add), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Div) },
            new[] { new Command(OpEnum.Const, 4.0f), new Command(OpEnum.Load, 0), new Command(OpEnum.Div), new Command(OpEnum.Dup), new Command(OpEnum.Add) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Copy, 0), new Command(OpEnum.Paste, 0), new Command(OpEnum.Add), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Mul) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Mul), new Command(OpEnum.Const, 3.0f), new Command(OpEnum.Mul), new Command(OpEnum.Const, 0.5f), new Command(OpEnum.Mul) },
            new[] { new Command(OpEnum.Const, 8.0f), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Sub), new Command(OpEnum.Load, 0), new Command(OpEnum.Mul), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Div) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Const, -1.0f), new Command(OpEnum.Mul), new Command(OpEnum.Const, 1.0f), new Command(OpEnum.Add), new Command(OpEnum.Load, 1), new Command(OpEnum.Mul) },
            new[] { new Command(OpEnum.Load, 0), new Command(OpEnum.Load, 1), new Command(OpEnum.Swap), new Command(OpEnum.Mul), new Command(OpEnum.Const, 2.0f), new Command(OpEnum.Add) },
        };
    }

    [Test]
    public void StdScoringMatchesCPUExactlyOnExactInputs()
    {
        // Inputs/outputs chosen from power-of-two set: all float ops and the reward math are exact,
        // so GPU and CPU must agree bit-for-bit. This is the strict guard for kernel equivalence.
        var organisms = ExactBuildOrganisms();
        var organismCount = organisms.Length;

        var powerOfTwo = new[] { -8f, -4f, -2f, -1f, -0.5f, 0f, 0.5f, 1f, 2f, 4f, 8f };
        var cpuRewards = new int[organismCount];
        var inputs = new float[Experiments * InputsCount];
        var correctOutputs = new float[Experiments];
        var rnd = new Random(998877);
        for (var e = 0; e < Experiments; e++)
        {
            inputs[e * InputsCount] = powerOfTwo[rnd.Next(powerOfTwo.Length)];
            inputs[e * InputsCount + 1] = powerOfTwo[rnd.Next(powerOfTwo.Length)];
            inputs[e * InputsCount + 2] = 0;
            correctOutputs[e] = powerOfTwo[rnd.Next(powerOfTwo.Length)];
        }

        var codeMachine = new CodeMachine();
        var fit = new StdFitFunc();
        for (var o = 0; o < organismCount; o++)
        {
            var sum = 0;
            for (var e = 0; e < Experiments; e++)
            {
                var inputSlice = new float[InputsCount];
                Array.Copy(inputs, (int)(e * InputsCount), inputSlice, 0, (int)InputsCount);
                var output = codeMachine.RunCommands(inputSlice, organisms[o]);
                if (!output.IsValidNumber() || !correctOutputs[e].IsValidNumber())
                {
                    sum += fit.FitFunctionIfInvalid(output.IsValidNumber(), correctOutputs[e].IsValidNumber());
                    continue;
                }
                sum += fit.FitFunction(output, correctOutputs[e]);
            }
            cpuRewards[o] = sum;
        }

        using var context = Context.Create(builder => builder.Default().EnableAlgorithms());
        var device = context.Devices.FirstOrDefault(d => d.AcceleratorType == AcceleratorType.Cuda) ?? context.Devices.First();
        using var acc = device.CreateAccelerator(context);
        var kernel = acc.LoadKernel<uint, ArrayView<int>, ArrayView<Command>, uint, ArrayView<float>, uint, ArrayView<float>, ArrayView<int>, StdFitFunc>(MainKernel.Kernel);

        var scriptStarts = new int[organismCount];
        var allCommands = new List<Command>();
        for (var o = 0; o < organismCount; o++)
        {
            scriptStarts[o] = allCommands.Count;
            allCommands.AddRange(organisms[o]);
        }

        using var dScriptStarts = acc.Allocate1D(scriptStarts);
        using var dCommands = acc.Allocate1D(allCommands.ToArray());
        using var dInputs = acc.Allocate1D(inputs);
        using var dCorrect = acc.Allocate1D(correctOutputs);
        using var dRewards = acc.Allocate1D<int>(organismCount);
        var stream = acc.DefaultStream;

        //Grid-stride std loop must be bit-exact for any power-of-two block size (Patch C).
        foreach (var groupSize in new[] { 512, 256, 128 })
        {
            dRewards.View.MemSetToZero(stream);
            kernel(stream, new KernelConfig(new Index1D(organismCount), new Index1D(groupSize)), Experiments,
                   dScriptStarts.View, dCommands.View, 0u, dInputs.View, InputsCount, dCorrect.View, dRewards.View, new StdFitFunc());
            stream.Synchronize();
            var gpuRewards = new int[organismCount];
            dRewards.View.CopyToCPU(stream, gpuRewards);

            for (var o = 0; o < organismCount; o++)
            {
                Assert.That(gpuRewards[o], Is.EqualTo(cpuRewards[o]), $"organism {o} mismatch on exact inputs, groupSize={groupSize}");
            }
        }
    }
}
