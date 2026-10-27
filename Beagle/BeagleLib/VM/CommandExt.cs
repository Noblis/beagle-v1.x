using System.Text;

namespace BeagleLib.VM;

public static class CommandExt
{
    public static int GetMaxInputIdx(this Command[] me)
    {
        var maxInputIdx = 0;
        for (var i = 0; i < me.Length; i++)
        {
            if (me[i].Operation == OpEnum.Load && me[i].Idx > maxInputIdx) maxInputIdx = me[i].Idx;
        }
        return maxInputIdx;
    }
    public static StringBuilder AppendToStringBuilder(this Command me, string[] inputLabels, StringBuilder sb)
    {
        switch (me.CommandType)
        {
            case CommandTypeEnum.CommandOnly:
            {
                sb.Append(me.Operation.GetUpperCase());
                break;
            }
            case CommandTypeEnum.CommandPlusFloat:
            {
                sb.Append(me.Operation.GetUpperCase());
                sb.Append(" ");
                sb.Append(me.ConstValue);
                break;
            }
            case CommandTypeEnum.CommandPlusIndex:
            {
                if (me.Operation == OpEnum.Copy || me.Operation == OpEnum.Paste)
                {
                    //copy and paste
                    sb.Append(me.Operation.GetUpperCase());
                    sb.Append(" @");
                    sb.Append(me.Idx);
                }
                else
                {
                    //loads
                    sb.Append(me.Operation.GetUpperCase());
                    sb.Append(" ");
                    sb.Append(inputLabels[me.Idx]);
                    sb.Append(":");
                    sb.Append(me.Idx);
                }
                break;
            }
            default: throw new Exception($"Unknown CommandType {me.CommandType}");
        }
        return sb;
    }
    public static StringBuilder AppendToStringBuilderWithGenericLabels(this Command me, StringBuilder sb)
    {
        switch (me.CommandType)
        {
            case CommandTypeEnum.CommandOnly:
            {
                sb.Append(me.Operation.GetUpperCase());
                break;
            }
            case CommandTypeEnum.CommandPlusFloat:
            {
                sb.Append(me.Operation.GetUpperCase());
                sb.Append(" ");
                sb.Append(me.ConstValue);
                break;
            }
            case CommandTypeEnum.CommandPlusIndex:
            {
                if (me.Operation == OpEnum.Copy || me.Operation == OpEnum.Paste)
                {
                    //copy and paste
                    sb.Append(me.Operation.GetUpperCase());
                    sb.Append(" @");
                    sb.Append(me.Idx);
                }
                else
                {
                    //loads
                    sb.Append(me.Operation.GetUpperCase());
                    sb.Append(" ");
                    sb.Append($"x{me.Idx+1}");
                    sb.Append(":");
                    sb.Append(me.Idx);
                }
                break;
            }
            default: throw new Exception($"Unknown CommandType {me.CommandType}");
        }
        return sb;
    }
}