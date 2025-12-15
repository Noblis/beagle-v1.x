using System.Diagnostics;
using System.Runtime.CompilerServices;
using BeagleLib.Util;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime.Cuda;

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
            else Execute(commands[i], false);
        }
        Debug.Assert(StackPointer == 1);
        return StackPop();
    }

    public float RunCommands(ArrayView<float> inputs, ArrayView<Command> commands, bool useLibDevice)
    {
        Stack = new float[BConfig.StackSize];
        StackPointer = 0;
        Clipboard = new ClipboardItem[BConfig.ClipboardSize];
        
        Inputs = inputs;

        for (var i = 0; i < commands.Length; i++)
        {
            Execute(commands[i], useLibDevice);
        }
        Debug.Assert(StackPointer == 1);
        return StackPop();
    }
    #endregion

    #region Private Methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Execute(Command command, bool useLibDevice)
    {
        switch (command.Operation, useLibDevice)
        {
            case (OpEnum.Add, _): ExecuteAdd(); return;
            case (OpEnum.Const, _): ExecuteConst(command.ConstValue); return;
            case (OpEnum.Div, _): ExecuteDiv(); return;
            case (OpEnum.Dup, _): ExecuteDup(); return;
            case (OpEnum.Del, _): ExecuteDel(); return;
            case (OpEnum.Load, _): ExecuteLoad(command.Idx); return;
            case (OpEnum.Mul, _): ExecuteMul(); return;
            case (OpEnum.Sign, _): ExecuteSign(); return;

            case (OpEnum.Sqrt, false): ExecuteSqrt(); return;
            case (OpEnum.Sqrt, true): ExecuteSqrtWithLibDevice(); return;

            case (OpEnum.Cbrt, false): ExecuteCbrt(); return;
            case (OpEnum.Cbrt, true): ExecuteCbrtWithLibDevice(); return;
            
            case (OpEnum.Sub, _): ExecuteSub(); return;
            case (OpEnum.Swap, _): ExecuteSwap(); return;
            case (OpEnum.Copy, _): ExecuteCopy(command.Idx); return;
            case (OpEnum.Paste, _): ExecutePaste(command.Idx); return;
            case (OpEnum.Square, _): ExecuteSquare(); return;
            case (OpEnum.Cube, _): ExecuteCube(); return;

            case (OpEnum.Ln, false): ExecuteLn(); return;
            case (OpEnum.Ln, true): ExecuteLnWithLibDevice(); return;

            case (OpEnum.Sin, false): ExecuteSin(); return;
            case (OpEnum.Sin, true): ExecuteSinWithLibDevice(); return;

            case (OpEnum.Pow, false): ExecutePow(); return;
            case (OpEnum.Pow, true): ExecutePowWithLibDevice(); return;

            //case (OpEnum.Abs, false): ExecuteAbs(); return;
            //case (OpEnum.Abs, true): ExecuteAbsWithLibDevice(); return;

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
        x = XMath.Sqrt(x);
        StackPush(x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSqrtWithLibDevice()
    {
        var x = StackPop();
        x = LibDevice.Sqrt(x);
        StackPush(x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteCbrt()
    {
        var x = StackPop();
        x = (float)Cbrt(x);
        StackPush(x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteCbrtWithLibDevice()
    {
        var x = StackPop();
        x = LibDevice.Cbrt(x);
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
        x = XMath.Log(x);
        StackPush(x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteLnWithLibDevice()
    {
        var x = StackPop();
        x = LibDevice.Log(x);
        StackPush(x);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSin()
    {
        var x = StackPop();
        x = XMath.Sin(x);
        StackPush(x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteSinWithLibDevice()
    {
        var x = StackPop();
        x = LibDevice.Sin(x);
        StackPush(x);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecutePow()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(XMath.Pow(y, x));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecutePowWithLibDevice()
    {
        var x = StackPop();
        var y = StackPop();
        StackPush(LibDevice.Pow(y, x));
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private void ExecuteAbs()
    //{
    //    var x = StackPop();
    //    x = XMath.Abs(x);
    //    StackPush(x);
    //}
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private void ExecuteAbsWithLibDevice()
    //{
    //    var x = StackPop();
    //    x = LibDevice.Abs(x);
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
    private double Cbrt(double x)
    {
        const int b1 = 715094163; /* B1 = (1023-1023/3-0.03306235651)*2**20 */
        const int b2 = 696219795; /* B2 = (1023-1023/3-54/3-0.03306235651)*2**20 */

        const double p0 = 1.87595182427177009643; /* 0x3ffe03e6, 0x0f61e692 */
        const double p1 = -1.88497979543377169875; /* 0xbffe28e0, 0x92f02420 */
        const double p2 = 1.621429720105354466140; /* 0x3ff9f160, 0x4a49d6c2 */
        const double p3 = -0.758397934778766047437; /* 0xbfe844cb, 0xbee751d9 */
        const double p4 = 0.145996192886612446982; /* 0x3fc2b000, 0xd4e4edd7 */

        double x1P54 = Interop.IntAsFloat(0x4350000000000000); // 0x1p54 === 2 ^ 54
        ulong ui = Interop.FloatAsInt(x);
        double r;
        double s;
        double t;
        double w;
        uint hx = ((uint)(ui >> 32)) & 0x7fffffff;

        if (hx >= 0x7ff00000)
        {
            /* cbrt(NaN,INF) is itself */
            return x + x;
        }

        /*
         * Rough cbrt to 5 bits:
         *    cbrt(2**e*(1+m) ~= 2**(e/3)*(1+(e%3+m)/3)
         * where e is integral and >= 0, m is real and in [0, 1), and "/" and
         * "%" are integer division and modulus with rounding towards minus
         * infinity.  The RHS is always >= the LHS and has a maximum relative
         * error of about 1 in 16.  Adding a bias of -0.03306235651 to the
         * (e%3+m)/3 term reduces the error to about 1 in 32. With the IEEE
         * floating point representation, for finite positive normal values,
         * ordinary integer divison of the value in bits magically gives
         * almost exactly the RHS of the above provided we first subtract the
         * exponent bias (1023 for doubles) and later add it back.  We do the
         * subtraction virtually to keep e >= 0 so that ordinary integer
         * division rounds towards minus infinity; this is also efficient.
         */
        if (hx < 0x00100000)
        {
            /* zero or subnormal? */
            ui = Interop.FloatAsInt(x * x1P54);
            hx = (uint)(ui >> 32) & 0x7fffffff;
            if (hx == 0) return x; /* cbrt(0) is itself */
            hx = hx / 3 + b2;
        }
        else
        {
            hx = hx / 3 + b1;
        }
        ui &= (ulong)1 << 63;
        ui |= (ulong)hx << 32;
        t = Interop.IntAsFloat(ui);

        /*
         * New cbrt to 23 bits:
         *    cbrt(x) = t*cbrt(x/t**3) ~= t*P(t**3/x)
         * where P(r) is a polynomial of degree 4 that approximates 1/cbrt(r)
         * to within 2**-23.5 when |r - 1| < 1/10.  The rough approximation
         * has produced t such than |t/cbrt(x) - 1| ~< 1/32, and cubing this
         * gives us bounds for r = t**3/x.
         *
         * Try to optimize for parallel evaluation as in __tanf.c.
         */
        r = (t * t) * (t / x);
        t = t * ((p0 + r * (p1 + r * p2)) + ((r * r) * r) * (p3 + r * p4));

        /*
         * Round t away from zero to 23 bits (sloppily except for ensuring that
         * the result is larger in magnitude than cbrt(x) but not much more than
         * 2 23-bit ulps larger).  With rounding towards zero, the error bound
         * would be ~5/6 instead of ~4/6.  With a maximum error of 2 23-bit ulps
         * in the rounded t, the infinite-precision error in the Newton
         * approximation barely affects third digit in the final error
         * 0.667; the error in the rounded t can be up to about 3 23-bit ulps
         * before the final error is larger than 0.667 ulps.
         */
        ui = Interop.FloatAsInt(t);
        ui = (ui + 0x80000000) & 0xffffffffc0000000;
        t = Interop.IntAsFloat(ui);

        /* one-step Newton iteration to 53 bits with error < 0.667 ulps */
        s = t * t; /* t*t is exact */
        r = x / s; /* error <= 0.5 ulps; |r| < |t| */
        w = t + t; /* t+t is exact */
        r = (r - t) / (w + r); /* r-t is exact; w+r ~= 3*t */
        t = t + t * r; /* error <= 0.5 + 0.5/3 + epsilon */
        return t;
    }
    #endregion

    #region Properties
    public ArrayView<float> Inputs { get; private set; }

    private float[] Stack { get; set; }
    private int StackPointer { get; set; }

    private ClipboardItem[] Clipboard { get; set; }
    #endregion
}