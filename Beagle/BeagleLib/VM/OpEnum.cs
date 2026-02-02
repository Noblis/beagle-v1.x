namespace BeagleLib.VM;

public enum OpEnum : byte
{
    //Copy is last for a reason. All adjunct commands should be at the end 
    EndOfScript = 0, Add, Const, Div, Dup, Del, Load, Mul, Sign, Sqrt, Cbrt, Sub, Swap, Paste, Square, Cube, Ln, 
    Sin, Cos, 
    Tan, Arcsin, Arcos, Arctan, Tanh, Exp,
    Pow,  /*Abs, Round,*/ //normal commands
    Copy //adjunct commands
}