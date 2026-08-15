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
        Stream.Dispose();
        DevScriptStarts.Dispose();
        DevCommands.Dispose();
        DevRewards.Dispose();
    }
    #endregion 

    #region Properties
    public Accelerator Accelerator { get; set; } = null!;

    public uint GroupSize { get; set; }
    public long MaxCommandBufferSize { get; set; }
    public long MaxDeviceCommandBufferSize { get; set; }
    public Command[] AllCommands { get; set; } = null!;
    public int[] ScriptStarts { get; set; } = null!;

    public MemoryBuffer1D<float, Stride1D.Dense> AllInputs { get; set; } = null!;
    public MemoryBuffer1D<float, Stride1D.Dense> CorrectOutputs { get; set; } = null!;

    //Persistent per-accelerator GPU resources (Patch A): allocated once at engine init, reused every scoring batch.
    public AcceleratorStream Stream { get; set; } = null!;
    public MemoryBuffer1D<int, Stride1D.Dense> DevScriptStarts { get; set; } = null!;
    public MemoryBuffer1D<Command, Stride1D.Dense> DevCommands { get; set; } = null!;
    public MemoryBuffer1D<int, Stride1D.Dense> DevRewards { get; set; } = null!;

    public Action<AcceleratorStream, KernelConfig, uint, ArrayView<int>, ArrayView<Command>, uint, ArrayView<float>, uint, ArrayView<float>, ArrayView<int>, TFitFunc> Kernel { get; set; } = null!;
    #endregion
}