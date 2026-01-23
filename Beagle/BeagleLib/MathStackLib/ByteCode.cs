namespace BeagleLib.MathStackLib;

public abstract record ByteCode
{
    #region Properties

    #region unary
    public sealed record Sign : ByteCode;
    public sealed record Sqrt : ByteCode;
    public sealed record Cbrt : ByteCode;
    public sealed record Square : ByteCode;
    public sealed record Cube : ByteCode;
    public sealed record Ln : ByteCode;
    public sealed record Sin : ByteCode;
    public sealed record Cos : ByteCode;
    //public sealed record Abs : ByteCode;
    // Don't know how to or if we want to represent rounding
    // public sealed record Round : Bytecode;
    #endregion unary

    #region binary
    public sealed record Add : ByteCode;
    public sealed record Sub : ByteCode;
    public sealed record Div : ByteCode;
    public sealed record Mul : ByteCode;
    public sealed record Pow : ByteCode;
    #endregion binary

    #region special
    // We need some sort of representation so we can switch between this name and a variable
    public sealed record Load(string Name) : ByteCode;
    public sealed record Const(float Value) : ByteCode;
    public sealed record Copy(int Idx) : ByteCode;
    public sealed record Paste(int Idx) : ByteCode;
    public sealed record Dup : ByteCode;
    public sealed record Swap : ByteCode;
    public sealed record Del : ByteCode;
    #endregion

    #endregion
}