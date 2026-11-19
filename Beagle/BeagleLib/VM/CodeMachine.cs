using System.Diagnostics;
using System.Runtime.CompilerServices;
using BeagleLib.Util;
using ILGPU;

namespace BeagleLib.VM;

public struct CodeMachine
{
    #region Methods
    [NotInsideKernel]
    public float RunCommands(float[] inputsArr, Command[] commands)
    {
        Stack = new float[BConfig.StackSize];
        StackPointer = 0;
        Clipboard = new ClipboardItem[BConfig.ClipboardSize];

        for (var i = 0; i < commands.Length; i++)
        {
            //we do special handling for Load command because we do not have ArrayView setup. Otherwise, we run normal Execute with no LibDevice
            if (commands[i].Operation == OpEnum.Load) ExecuteLoadFromArray(inputsArr, commands[i].Idx);
            else Execute(commands[i]);
        }
        Debug.Assert(StackPointer == 1);
        return StackPop();
    }

    public float RunCommands(ArrayView<float> inputs, ArrayView<Command> commands)
    {
        Stack = new float[BConfig.StackSize];
        StackPointer = 0;
        Clipboard = new ClipboardItem[BConfig.ClipboardSize];
        
        Inputs = inputs;

        for (var i = 0; i < commands.Length; i++)
        {
            Execute(commands[i]);
        }
        Debug.Assert(StackPointer == 1);
        return StackPop();
    }
    #endregion

    #region Private Methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Execute(Command command)
    {
        switch (command.Operation)
        {
            case OpEnum.Add: ExecuteAdd(); return;
            case OpEnum.Const: ExecuteConst(command.ConstValue); return;
            case OpEnum.Div: ExecuteDiv(); return;
            case OpEnum.Dup: ExecuteDup(); return;
            case OpEnum.Del: ExecuteDel(); return;
            case OpEnum.Load: ExecuteLoad(command.Idx); return;
            case OpEnum.Mul: ExecuteMul(); return;
            case OpEnum.Sign: ExecuteSign(); return;

            case OpEnum.Sqrt: ExecuteSqrt(); return;

            case OpEnum.Cbrt: ExecuteCbrt(); return;
            
            case OpEnum.Sub: ExecuteSub(); return;
            case OpEnum.Swap: ExecuteSwap(); return;
            case OpEnum.Copy: ExecuteCopy(command.Idx); return;
            case OpEnum.Paste: ExecutePaste(command.Idx); return;
            case OpEnum.Square: ExecuteSquare(); return;
            case OpEnum.Cube: ExecuteCube(); return;

            case OpEnum.Ln: ExecuteLn(); return;

            case OpEnum.Sin: ExecuteSin(); return;

            case OpEnum.Cos: ExecuteCos(); return;

            case OpEnum.Tan: ExecuteTan(); return;

            case OpEnum.Arccos: ExecuteArccos(); return;

            case OpEnum.Arcsin: ExecuteArcsin(); return;

            case OpEnum.Arctan: ExecuteArctan(); return;

            case OpEnum.Tanh: ExecuteTanh(); return;

            case OpEnum.Exp: ExecuteExp(); return;

            case OpEnum.Pow: ExecutePow(); return;

            //case (OpEnum.Abs, false): ExecuteAbs(); return;

            //case OpEnum.Round: ExecuteRound(); return;

            default: Debug.Assert(false, "Unknown OperationEnum"); return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteAdd()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(x + y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteConst(float constValue)
    {
        StackPush(constValue);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteDiv()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(y / x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteDup()
    {
        var x = StackPeek();
        StackPush(x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteDel()
    {
        StackPop();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteLoad(int idx)
    {
        StackPush(Inputs[idx]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining), NotInsideKernel]
    private void ExecuteLoadFromArray(float[] inputs, int idx)
    {
        StackPush(inputs[idx]);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteMul()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(x * y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSign()
    {
        var x = StackPop();
        StackPush(-x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSqrt()
    {
        var x = StackPop();
        x = MathF.Sqrt(x);
        StackPush(x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteCbrt()
    {
        var x = StackPop();
        x = Cbrt(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSub()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(y - x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSwap()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(x);
        StackPush(y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteCopy(int idx)
    {
        var x = StackPeek();
        PushToClipboard(idx, x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecutePaste(int idx)
    {
        var x = PopFromClipboard(idx);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StackClear()
    {
        StackPointer = 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StackPush(float x)
    {
        Debug.Assert(StackPointer < BConfig.StackSize);
        Stack[StackPointer] = x;
        StackPointer++;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float StackPop()
    {
        Debug.Assert(StackPointer > 0);
        StackPointer--;
        return Stack[StackPointer];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float StackPeek()
    {
        Debug.Assert(StackPointer > 0);
        return Stack[StackPointer - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushToClipboard(int idx, float val)
    {
        for (var i = 0; i < Clipboard.Length; i++)
        {
            if (!Clipboard[i].InUse)
            {
                Clipboard[i].InUse = true;
                Clipboard[i].Index = idx;
                Clipboard[i].Value = val;
                return;
            }
        }
        Debug.Assert(false, "Unable to write to clipboard because it is full");
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float PopFromClipboard(int idx)
    {
        for (var i = 0; i < Clipboard.Length; i++)
        {
            if (Clipboard[i].InUse && Clipboard[i].Index == idx)
            {
                Clipboard[i].InUse = false;
                return Clipboard[i].Value;
            }
        }
        Debug.Assert(false, "Unable to find variable in clipboard");
        return float.NaN;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSquare()
    {
        var x = StackPop();
        StackPush(x * x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteCube()
    {
        var x = StackPop();
        StackPush(x * x * x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteLn()
    {
        var x = StackPop();
        x = MathF.Log(x);
        StackPush(x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSin()
    {
        var x = StackPop();
        x = MathF.Sin(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteCos()
    {
        var x = StackPop();
        x = MathF.Cos(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteTan()
    {
        var x = StackPop();
        //x = MathF.Tan(x);
        x = MathF.Sin(x) / MathF.Cos(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteArccos()
    {
        var x = StackPop();
        x = MathF.Acos(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteArcsin()
    {
        var x = StackPop();
        x = MathF.Asin(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteArctan()
    {
        var x = StackPop();
        x = MathF.Atan(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteTanh()
    {
        var x = StackPop();
        x = MathF.Tanh(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteExp()
    {
        var x = StackPop();
        x = MathF.Exp(x);
        StackPush(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecutePow()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(MathF.Pow(y, x));
    }


    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private void ExecuteAbs()
    //{
    //    var x = StackPop();
    //    x = MathF.Abs(x);
    //    StackPush(x);
    //}
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private void ExecuteRound()
    //{
    //    var x = StackPop();
    //    StackPush((int)(x + 0.5));
    //}

    //This is a port from Rust https://github.com/rust-lang/libm/blob/master/src/math/cbrt.rs
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    private static float Cbrt(float x)
    {
        const uint b1 = 709958130;
        const uint b2 = 642849266;

        uint bits = (uint)BitConverter.SingleToInt32Bits(x);
        uint hx = bits & 0x7FFFFFFF;

        // NaN or infinity
        if (hx >= 0x7F800000) return x + x;

        // Preserve positive and negative zero
        if (hx == 0) return x;

        // Initial approximation (~5 bits)
        if (hx < 0x00800000)
        {
            // Subnormal: multiply by 2^24
            float scaled = x * 16777216.0f;
            hx = (uint)BitConverter.SingleToInt32Bits(scaled);
            hx = (hx & 0x7FFFFFFF) / 3 + b2;
        }
        else
        {
            hx = hx / 3 + b1;
        }

        // Restore sign
        bits = (bits & 0x80000000) | hx;
        double t = BitConverter.Int32BitsToSingle(unchecked((int)bits));
        double xd = x;

        // First iteration: approximately 16 bits
        double r = t * t * t;
        t = t * (2.0 * xd + r) / (xd + 2.0 * r);

        // Second iteration: approximately 47 bits
        r = t * t * t;
        t = t * (2.0 * xd + r) / (xd + 2.0 * r);

        return (float)t;
    }
    #endregion

    #region Properties
    public ArrayView<float> Inputs { get; private set; }

    private float[] Stack { get; set; }
    private int StackPointer { get; set; }

    private ClipboardItem[] Clipboard { get; set; }
    #endregion
}