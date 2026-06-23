using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.Serialization;
using Supermodel.DataAnnotations.Attributes;

namespace Supermodel.ReflectionMapper;

public static class ObjectAttributeReaderExtensions
{
    public static string GetDescription(this object? value)
    {
        if (value == null) return "";
            
        //Tries to find a DescriptionAttribute for a potential friendly name for the enum
        var type = value.GetType();
        if (type.IsEnum)
        {
            if (_enumDescDict.ContainsKey((Enum)value)) return _enumDescDict[(Enum)value];

            var memberInfo = type.GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                var attr = Attribute.GetCustomAttribute(memberInfo[0], typeof(DescriptionAttribute), true);
                if (attr != null)
                {
                    var result = _enumDescDict[(Enum)value] = ((DescriptionAttribute)attr).Description;
                    return result;
                }
            }
            //If we have no description attribute, just return the ToString() or ToString().InsertSpacesBetweenWords() for enum
            var valueToString = value.ToString().InsertSpacesBetweenWords();
            _enumDescDict[(Enum)value] = valueToString;
            return valueToString;
        }

        return value.ToString();
    }
    private static readonly ConcurrentDictionary<Enum, string> _enumDescDict = new();

    public static int GetScreenOrder(this object value)
    {
        //Tries to find a ScreenOrderAttribute for a potential friendly name for the enum
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        if (memberInfo.Length > 0)
        {
            var attr = Attribute.GetCustomAttribute(memberInfo[0], typeof(ScreenOrderAttribute), true);
            if (attr != null) return ((ScreenOrderAttribute)attr).Order;
        }
        //If we have no order, default is 100
        return 100;
    }

    public static bool IsDisabled(this object value)
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        if (memberInfo.Length > 0)
        {
            var attr = Attribute.GetCustomAttribute(memberInfo[0], typeof(DisabledAttribute), true);
            if (attr != null) return true;
        }
        //If we have no disabled attribute, we assume active
        return false;
    }
        
    public static string? GetEnumMemberAttributeValueOrNull(this Enum value)
    {
        if (_enumMemberDict.TryGetValue(value, out var @null)) return @null;

        //Tries to find a EnumMemberAttribute for a potential serialization name for the enum
        var type = value.GetType();

        var memberInfo = type.GetMember(value.ToString());
        if (memberInfo.Length > 0)
        {
            var attr = Attribute.GetCustomAttribute(memberInfo[0], typeof(EnumMemberAttribute), true);
            if (attr != null)
            {
                var result = _enumMemberDict[value] = ((EnumMemberAttribute)attr).Value;
                return result;
            }
        }
        //If we have no EnumMemberAttribute attribute, just return null
        _enumMemberDict[value] = null;
        return null;
    }
    private static readonly ConcurrentDictionary<Enum, string?> _enumMemberDict = new();
}