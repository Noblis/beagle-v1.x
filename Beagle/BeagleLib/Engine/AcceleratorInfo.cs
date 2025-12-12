using BeagleLib.Engine.FitFunc;
using BeagleLib.VM;
using ILGPU;
using ILGPU.Runtime;

namespace BeagleLib.Engine;

public class AcceleratorInfo<TFitFunc> : IDisposable where TFitFunc : struct, IFitFunc
{
    #region IDisposable implemetation
    public void Dispose()
    {
        Accelerator.Dispose();
        AllInputs.Dispose();
        CorrectOutputs.Dispose();
    }
    #endregion 

    #region Properties
    public Accelerator Accelerator { get; set; } = null!;

    public uint GroupSize { get; set; }
    public long MaxCommandBufferSize { get; set; }
    public Command[] AllCommands { get; set; } = null!;
    public int[] ScriptStarts { get; set; } = null!;

    public MemoryBuffer1D<float, Stride1D.Dense> AllInputs { get; set; } = null!;
    public MemoryBuffer1D<float, Stride1D.Dense> CorrectOutputs { get; set; } = null!;

    public Action<AcceleratorStream, KernelConfig, byte, uint, ArrayView<int>, ArrayView<Command>, uint, ArrayView<float>, uint, ArrayView<float>, ArrayView<int>, TFitFunc> Kernel { get; set; } = null!;
    #endregion
}