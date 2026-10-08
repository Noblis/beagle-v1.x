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
        AllInputsMB.Dispose();
        CorrectOutputsMB.Dispose();
        Stream.Dispose();
    }
    #endregion 

    #region Properties
    public Accelerator Accelerator { get; set; } = null!;

    public uint GroupSize { get; set; }
    public long MaxCommandBufferSize { get; set; }
    public Command[] AllCommands { get; set; } = null!;
    public int[] ScriptStarts { get; set; } = null!;

    public MemoryBuffer1D<float, Stride1D.Dense> AllInputsMB { get; set; } = null!;
    public MemoryBuffer1D<float, Stride1D.Dense> CorrectOutputsMB { get; set; } = null!;

    public AcceleratorStream Stream { get; set; } = null!;

    public Action<AcceleratorStream, KernelConfig, uint, ArrayView<int>, ArrayView<Command>, uint, ArrayView<float>, uint, ArrayView<float>, ArrayView<int>, TFitFunc> Kernel { get; set; } = null!;
    #endregion
}