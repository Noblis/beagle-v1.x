using System.Diagnostics;

namespace BeagleLib.VM;

public static class OperationEnumHelper
{
    public static OpProps GetOperationProperties(this OpEnum me)
    {
        switch (me)
        {
            case OpEnum.EndOfScript:
                Debug.Assert(false, "Trying to get operation properties for OpEnum.EndOfScript");
                return new OpProps(CommandTypeEnum.CommandOnly, -100, 100);

            case OpEnum.Add: return new OpProps(CommandTypeEnum.CommandOnly, -1, 2);
            case OpEnum.Const: return new OpProps(CommandTypeEnum.CommandPlusFloat, 1, 0);
            case OpEnum.Div: return new OpProps(CommandTypeEnum.CommandOnly, -1, 2);
            case OpEnum.Dup: return new OpProps(CommandTypeEnum.CommandOnly, 1, 1);
            case OpEnum.Del: return new OpProps(CommandTypeEnum.CommandOnly, -1, 1);
            case OpEnum.Load: return new OpProps(CommandTypeEnum.CommandPlusIndex, 1, 0);
            case OpEnum.Mul: return new OpProps(CommandTypeEnum.CommandOnly, -1, 2);
            case OpEnum.Sign: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Sqrt: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Cbrt: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Sub: return new OpProps(CommandTypeEnum.CommandOnly, -1, 2);
            case OpEnum.Swap: return new OpProps(CommandTypeEnum.CommandOnly, 0, 2);
            case OpEnum.Copy: return new OpProps(CommandTypeEnum.CommandPlusIndex, 0, 1);
            case OpEnum.Paste: return new OpProps(CommandTypeEnum.CommandPlusIndex, 1, 0);
            case OpEnum.Square: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Cube: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Ln: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Sin: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Cos: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Tan: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Arccos: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Arcsin: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Arctan: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Tanh: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Exp: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            case OpEnum.Pow: return new OpProps(CommandTypeEnum.CommandOnly, -1, 2);

            //case OpEnum.Abs: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);
            //case OpEnum.Round: return new OpProps(CommandTypeEnum.CommandOnly, 0, 1);

            default:
                Debug.Assert(false, "GetCommandType. Unknown OpEnum");
                return new OpProps(CommandTypeEnum.CommandOnly, -100, 100);
        }
    }

    public static string GetUpperCase(this OpEnum me)
    {
        if (_upperCaseOpEnums == null)
        {
            //we create tempUpperCaseOpEnums for thread safety
            var tempUpperCaseOpEnums = new string[Enum.GetValues<OpEnum>().Length]; 
            foreach (var op in Enum.GetValues<OpEnum>())
            {
                tempUpperCaseOpEnums[(int)op] = op.ToString().ToUpper();
            }
            _upperCaseOpEnums = tempUpperCaseOpEnums;
        }
        return _upperCaseOpEnums[(int)me];
    }

    private static string[]? _upperCaseOpEnums;
}