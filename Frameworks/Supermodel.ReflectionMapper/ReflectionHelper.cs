using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Validations;

namespace Supermodel.ReflectionMapper;

public static class ReflectionHelper
{
    #region Methods
    public static void LoadAllAssemblies(this AppDomain me)
    {
        if (AllAssembliesLoaded)
        {
            AllAssembliesLoaded = true;
            lock (_loadAllAssembliesLock)
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName).ToHashSet();

                var assembliesToCheck = new Queue<Assembly>();
                assembliesToCheck.Enqueue(Assembly.GetEntryAssembly());

                while (assembliesToCheck.Any())
                {
                    var assemblyToCheck = assembliesToCheck.Dequeue();

                    foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
                    {
                        if (!loadedAssemblies.Contains(reference.FullName))
                        {
                            var assembly = Assembly.Load(reference);
                            assembliesToCheck.Enqueue(assembly);
                            loadedAssemblies.Add(reference.FullName);
                        }
                    }
                }
            }
        }
    }
    public static Assembly[] GetAllAssemblies(this AppDomain me)
    {
        me.LoadAllAssemblies();
        return AppDomain.CurrentDomain.GetAssemblies();
    }
        
    public static Task<object?> GetResultAsObjectAsync(this object task)
    {
        return ((Task)task).GetResultAsObjectAsync();
    }
    public static async Task<object?> GetResultAsObjectAsync(this Task task)
    {
        await task;
        return task.PropertyGet("Result");
    }
        
    public static string GetThrowingContext()
    {
        var stackFrame = new StackTrace().GetFrame(2);
        return stackFrame.GetMethod().DeclaringType + "::" + stackFrame.GetMethod().Name + "()";
    }
    public static string GetCurrentContext()
    {
        var stackFrame = new StackTrace().GetFrame(1);
        return stackFrame.GetMethod().DeclaringType + "::" + stackFrame.GetMethod().Name + "()";
    }
        
    public static object CreateType(Type type, params object?[] args)
    {
        if (type.GetTypeInfo().IsGenericType) return CreateGenericType(type.GetGenericTypeDefinition(), type.GetGenericArguments());
        else return Activator.CreateInstance(type, args);
    }
    public static object CreateGenericType(Type genericType, Type[] innerTypes, params object?[] args)
    {
        var specificType = genericType.MakeGenericType(innerTypes);
        return Activator.CreateInstance(specificType, args);
    }
    public static object CreateGenericType(Type genericType, Type innerType, params object?[] args)
    {
        return CreateGenericType(genericType, [innerType], args);
    }
        
    public static object? PropertyGet(this object me, string propertyName, object[]? index = null)
    {
        return me.GetPropertyInfo(propertyName)?.GetValue(me, index);
    }
    public static object? PropertyGetNonPublic(this object me, string propertyName, object[]? index = null)
    {
        return me.GetPropertyInfoNonPublic(propertyName)?.GetValue(me, index);
    }

    public static void PropertySet(this object me, string propertyName, object? newValue, bool ignoreNoSetMethod = false, object[]? index = null)
    {
        var myProperty = me.GetPropertyInfo(propertyName);
        if (myProperty == null) throw new ArgumentException($"{propertyName} property does not exist");
            
        var setMethod = myProperty.GetSetMethod(true);
        if (setMethod != null)
        {
            setMethod.Invoke(me, [newValue]);
        }
        else
        {
            if (!ignoreNoSetMethod) myProperty.SetValue(me, newValue, index);
        }
    }
    public static void PropertySetNonPublic(this object me, string propertyName, object? newValue, bool ignoreNoSetMethod = false, object[]? index = null)
    {
        var myProperty = me.GetPropertyInfoNonPublic(propertyName);
        if (myProperty == null) throw new ArgumentException($"{propertyName} property does not exist");

        if (ignoreNoSetMethod)
        {
            if (myProperty.GetSetMethod() != null) myProperty.SetValue(me, newValue, index);
        }
        else
        {
            myProperty.SetValue(me, newValue, index);
        }
    }
        
    public static object? ExecuteStaticMethod(Type typeofClassWithStaticMethod, string methodName, params object?[] args)
    {
        var methodInfo = typeofClassWithStaticMethod.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == methodName && !m.IsGenericMethod && m.GetParameters().Length == args.Length);
        return methodInfo.Invoke(null, args);
    }
    public static object? ExecuteNonPublicStaticMethod(Type typeofClassWithStaticMethod, string methodName, params object?[] args)
    {
        var methodInfo = typeofClassWithStaticMethod.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(m => m.Name == methodName && !m.IsGenericMethod && m.GetParameters().Length == args.Length);
        return methodInfo.Invoke(null, args);
    }
        
    public static object? ExecuteStaticGenericMethod(Type typeofClassWithGenericStaticMethod, string methodName, Type[] genericArguments, params object?[] args)
    {
        var methodInfo = typeofClassWithGenericStaticMethod.GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == methodName && m.IsGenericMethod && m.GetParameters().Length == args.Length);
        var genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);
        return genericMethodInfo.Invoke(null, args);
    }
    public static object? ExecuteNonPublicStaticGenericMethod(Type typeofClassWithGenericStaticMethod, string methodName, Type[] genericArguments, params object?[] args)
    {
        var methodInfo = typeofClassWithGenericStaticMethod.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(m => m.Name == methodName && m.IsGenericMethod && m.GetParameters().Length == args.Length);
        var genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);
        return genericMethodInfo.Invoke(null, args);
    }
        
    public static object? ExecuteGenericMethod(this object me, string methodName, Type[] genericArguments, params object?[] args)
    {
        try
        {
            var methodInfo = me.GetType().GetMethods().Single(m =>
                m.Name == methodName && m.IsGenericMethod && m.GetParameters().Length == args.Length &&
                m.GetParameters().Length == args.Length);
            var genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);
            return genericMethodInfo.Invoke(me, args);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is ValidationResultException) throw ex.InnerException;
            
            throw new ReflectionMethodCantBeInvoked(me.GetType(), methodName);
        }
    }
    public static object? ExecuteNonPublicGenericMethod(this object me, string methodName, Type[] genericArguments, params object?[] args)
    {
        try
        {
            var methodInfo = me.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == methodName && x.IsGenericMethod && x.GetParameters().Length == args.Length);
            var genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);
            return genericMethodInfo.Invoke(me, args);
        }
        catch (Exception)
        {
            throw new ReflectionMethodCantBeInvoked(me.GetType(), methodName);
        }
    }
        
    public static object? ExecuteMethod(this object me, string methodName, params object?[] args)
    {
        var method = me.GetType().GetMethods().SingleOrDefault(x => x.Name == methodName && x.GetParameters().Length == args.Length);
        if (method == null) throw new ReflectionMethodCantBeInvoked(me.GetType(), methodName);
        return method.Invoke(me, args);
    }
    public static object? ExecuteNonPublicMethod(this object me, string methodName, params object?[] args)
    {
        try
        {
            return me.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Single(x => x.Name == methodName && x.GetParameters().Length == args.Length).Invoke(me, args);
        }
        catch (Exception)
        {
            throw new ReflectionMethodCantBeInvoked(me.GetType(), methodName);
        }
    }

    public static bool IsClassADerivedFromClassB(Type a, Type b)
    {
        return IfClassADerivedFromClassBGetFullGenericBaseTypeOfB(a, b) != null;
    }
    public static Type? IfClassADerivedFromClassBGetFullGenericBaseTypeOfB(Type a, Type b)
    {
        if (a == b) return a;

        //var aBaseType = a.BaseType;
        var aBaseType = a.GetTypeInfo().BaseType;

        if (aBaseType == null) return null;

        if (b.GetTypeInfo().IsGenericTypeDefinition && aBaseType.GetTypeInfo().IsGenericType)
        {
            if (aBaseType.GetGenericTypeDefinition() == b) return aBaseType;
        }
        else
        {
            if (aBaseType == b) return aBaseType;
        }
        return IfClassADerivedFromClassBGetFullGenericBaseTypeOfB(aBaseType, b);
    }

    public static string GetTypeDescription(this Type type)
    {
        //var attr = MyAttribute.GetCustomAttribute(type, typeof(DescriptionAttribute), true);
        var attr = type.GetTypeInfo().GetCustomAttribute(typeof(DescriptionAttribute), true);
        return attr != null ? ((DescriptionAttribute)attr).Description : type.ToString().InsertSpacesBetweenWords();
    }
    public static string GetTypeFriendlyDescription(this Type type)
    {
        //var attr = MyAttribute.GetCustomAttribute(type, typeof(DescriptionAttribute), true);
        var attr = type.GetTypeInfo().GetCustomAttribute(typeof(DescriptionAttribute), true);
        return attr != null ? ((DescriptionAttribute)attr).Description : type.Name.InsertSpacesBetweenWords();
    }

    public static bool HasAttribute<TAttribute>(this MemberInfo element, bool inherit = true) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(element, typeof(TAttribute), inherit) != null;
    }
    public static TAttribute? GetAttribute<TAttribute>(this MemberInfo element, bool inherit = true) where TAttribute : Attribute
    {
        return (TAttribute?)Attribute.GetCustomAttribute(element, typeof(TAttribute), inherit);
    }

    public static object? PropertyGetWithNullIfNoProperty(this object me, string propertyName)
    {
        var propertyInfo = me.GetType().GetTypeInfo().DeclaredProperties.SingleOrDefault(x => x.Name == propertyName);
        return propertyInfo?.GetValue(me);
    }

    public static object? DefaultValue(this Type type)
    {
        if (type.IsValueType) return Activator.CreateInstance(type);
        return null;
    }

    public static bool IsNullable(Type type, PropertyInfo property)
    {
        if (!type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Contains(property)) throw new ArgumentException("enclosingType must be the type which defines property");

        if (property.PropertyType.IsValueType) return Nullable.GetUnderlyingType(property.PropertyType) != null;
            
        var nullable = property.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte)) return (byte)args[0].Value == 2;
            }
            if (attributeArgument.ArgumentType == typeof(byte)) return (byte)attributeArgument.Value == 2;
        }

        var typeNullableContext = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        if (typeNullableContext != null && typeNullableContext.ConstructorArguments.Count == 1 && typeNullableContext.ConstructorArguments[0].ArgumentType == typeof(byte)) return (byte)typeNullableContext.ConstructorArguments[0].Value == 2;

        return false; // Couldn't find a suitable attribute
    }
    public static bool IsNullable(Type type, MethodInfo method, ParameterInfo parameter)
    {
        if (!type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Contains(method)) throw new ArgumentException("type must be the type which defines method");
        if (!method.GetParameters().Contains(parameter)) throw new ArgumentException("method must be the method which defines parameter");

        if (parameter.ParameterType.IsValueType) return Nullable.GetUnderlyingType(parameter.ParameterType) != null;

        var nullable = parameter.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte)) return (byte)args[0].Value == 2;
            }
            if (attributeArgument.ArgumentType == typeof(byte)) return (byte)attributeArgument.Value == 2;
        }

        var methodNullableContext = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        if (methodNullableContext != null && methodNullableContext.ConstructorArguments.Count == 1 && methodNullableContext.ConstructorArguments[0].ArgumentType == typeof(byte)) return (byte)methodNullableContext.ConstructorArguments[0].Value == 2;

        var typeNullableContext = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        if (typeNullableContext != null && typeNullableContext.ConstructorArguments.Count == 1 && typeNullableContext.ConstructorArguments[0].ArgumentType == typeof(byte)) return (byte)typeNullableContext.ConstructorArguments[0].Value == 2;

        return false; // Couldn't find a suitable attribute
    }
    public static bool IsNullable(Type type, MethodInfo method)
    {
        if (!type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Contains(method)) throw new ArgumentException("type must be the type which defines method");
            
        var returnParameter = method.ReturnParameter;
        if (returnParameter == null) return false;
            
        if (returnParameter.ParameterType.IsValueType) return Nullable.GetUnderlyingType(returnParameter.ParameterType) != null;

        var nullable = returnParameter.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte)) return (byte)args[0].Value == 2;
            }
            if (attributeArgument.ArgumentType == typeof(byte)) return (byte)attributeArgument.Value == 2;
        }

        var methodNullableContext = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        if (methodNullableContext != null && methodNullableContext.ConstructorArguments.Count == 1 && methodNullableContext.ConstructorArguments[0].ArgumentType == typeof(byte)) return (byte)methodNullableContext.ConstructorArguments[0].Value == 2;

        var typeNullableContext = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        if (typeNullableContext != null && typeNullableContext.ConstructorArguments.Count == 1 && typeNullableContext.ConstructorArguments[0].ArgumentType == typeof(byte)) return (byte)typeNullableContext.ConstructorArguments[0].Value == 2;

        return false; // Couldn't find a suitable attribute
    }

    public static bool IsComplexType(this Type me)
    {
        if (me == typeof(string)) return false;
        //if (me == typeof(byte[])) return false;
        if (me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>)) return false;

        return (me.IsClass || me.IsStruct());
    }
    public static bool IsStruct(this Type me)
    {
        return me.IsValueType && !me.IsPrimitive && !me.IsEnum && me != typeof(decimal);
    }
    public static bool IsAnonymousType(this Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        // HACK: The only way to detect anonymous types right now.
        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }

    public static Type? GetICollectionGenericArg(this Type type)
    {
        //If the type is ICollection
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>)) return type.GetGenericArguments()[0];

        foreach (var @interface in type.GetInterfaces())
        {
            if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(ICollection<>)) return @interface.GetGenericArguments()[0];
        }

        return null;
    }
    #endregion

    #region Private Helpers
    private static PropertyInfo? GetPropertyInfoNonPublic(this object me, string propertyName)
    {
        return me.GetType().GetPropertyInfoNonPublicWithType(propertyName);
    }
    private static PropertyInfo? GetPropertyInfoNonPublicWithType(this Type myType, string propertyName)
    {
        try
        {
            return myType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).SingleOrDefault(p => p.Name == propertyName);
        }
        catch (Exception)
        {
            throw new ReflectionPropertyCantBeInvoked(myType, propertyName);
        }
    }

    private static PropertyInfo? GetPropertyInfo(this object me, string propertyName)
    {
        return me.GetType().GetPropertyInfoWithType(propertyName);
    }
    private static PropertyInfo? GetPropertyInfoWithType(this Type myType, string propertyName)
    {
        try
        {
            return myType.GetProperty(propertyName);
        }
        catch(Exception)
        {
            throw new ReflectionPropertyCantBeInvoked(myType, propertyName);
        }
    }
    #endregion

    #region Properties
    private static bool AllAssembliesLoaded { get; set; }
    private static object _loadAllAssembliesLock = new();
    #endregion
}