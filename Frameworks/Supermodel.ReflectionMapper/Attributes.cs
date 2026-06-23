using System;
using System.Linq;
using System.Text;

namespace Supermodel.ReflectionMapper;

[AttributeUsage(AttributeTargets.Property)]
public class NotRComparedAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class NotRMappedAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class NotRMappedToAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class NotRMappedFromAttribute : Attribute { }
    

[AttributeUsage(AttributeTargets.Property)]
public class RMCopyShallowAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class RMCopyShallowToAttribute : Attribute { }
    
[AttributeUsage(AttributeTargets.Property)]
public class RMCopyShallowFromAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Class)]
public class RMCopyAllPropsShallowAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class RMCopyAllPropsShallowToAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class RMCopyAllPropsShallowFromAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Property)]
public class RMapsToAttribute : Attribute
{
    #region Constructors
    //public RMapToAttribute() { }
    public RMapsToAttribute(string fullPath)
    {
        FullPath = fullPath;
    }
    #endregion

    #region Properties
    public string FullPath
    {
        set
        {
            if (!value.StartsWith(".")) throw new ReflectionMapperException($"{nameof(FullPath)}: Path must always start with a '.'");
            var pathParts = value.Split('.');
            var sb = new StringBuilder();
            for (var i = 1; i < pathParts.Length - 1; i++)
            {
                if (i == 1) sb.Append($"{pathParts[i]}");
                else sb.Append($".{pathParts[i]}");
            }
            ObjectPath = sb.ToString();
            PropertyName = value.EndsWith("*") ? null : pathParts.Last();
        }
    }
    public string ObjectPath { get; protected set; } = "";
    public string? PropertyName { get; protected set; }
    #endregion
}