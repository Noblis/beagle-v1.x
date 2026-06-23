using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Namotion.Reflection;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Misc;
using Supermodel.DataAnnotations.Validations;
using WebMonk.Context;
using WebMonk.Exceptions;
using WebMonk.Extensions;

namespace WebMonk.ModeBinding;

//https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md
//https://github.com/RicoSuter/Namotion.Reflection
public static class NullabilityHelper
{
    #region Methods
    public static bool TryValidateObjectNullability(object obj, ValidationResultList vrl)
    {
        ValidateObjectNullability(obj, obj, vrl);
        return vrl.IsValid;
    }
    #endregion

    #region Private Helper Methods
    private static void ValidateObjectNullability(object rootObj, object obj, ValidationResultList vrl)
    {
        var objType = obj.GetType();

        //case for IEnumerable
        if (obj is IEnumerable && !(obj is string))
        {
            if (obj is Array array) 
            {
                var mustNotBeNull = objType.ToContextualType().ElementType!.Nullability == Nullability.NotNullable;
                for(var i = 0; i < array.Length; i++)
                {
                    using(HttpContext.Current.PrefixManager.NewPrefix($"[{i}]", obj))
                    {
                        var item = array.GetValue(i);
                        ValidateItem(rootObj, item, mustNotBeNull, vrl);
                    }
                }
            }
            else if (objType.IsGenericType && obj is IList list) 
            {
                var mustNotBeNull = objType.ToContextualType().GenericArguments[0].Nullability == Nullability.NotNullable;
                for(var i = 0; i < list.Count; i++)
                {
                    using(HttpContext.Current.PrefixManager.NewPrefix($"[{i}]", obj))
                    {
                        var item = list[i];
                        ValidateItem(rootObj, item, mustNotBeNull, vrl);
                    }
                }
            }
            else if (objType.IsGenericType && obj is IDictionary dict) 
            {
                var mustNotBeNull = objType.ToContextualType().GenericArguments[0].Nullability== Nullability.NotNullable;
                foreach(var key in dict.Keys)
                {
                    using(HttpContext.Current.PrefixManager.NewPrefix($"[{key}]", obj))
                    {
                        var item = dict[key];
                        ValidateItem(rootObj, item, mustNotBeNull, vrl);
                    }
                }
            }
            else if (objType == typeof(AttributesDict))
            {
                //Ignore and do nothing
            }
            else 
            {
                throw new WebMonkException($"Unable to Validate nullability for {objType.Name} type, can only validate for generic IEnumerable types such as Arrays, Lists, and Dictionaries");
            }
        }

        //case for complex types
        else if (objType.IsComplexType())
        {
            foreach (var propertyInfo in objType.GetProperties().Where(x => x.GetMethod != null))
            {
                using (HttpContext.Current.PrefixManager.NewPrefix(propertyInfo.Name, obj))
                {
                    var item = propertyInfo.GetValue(obj);
                    var mustNotBeNull = propertyInfo.ToContextualProperty().Nullability == Nullability.NotNullable;
                    ValidateItem(rootObj, item, mustNotBeNull, vrl);
                }
            }
        }
    }
    private static void ValidateItem(object rootObj, object? item, bool mustNotBeNull, ValidationResultList vrl)
    {
        if (item != null)
        {
            if (item is DateTime) return;
            if (item.GetType().IsComplexType()) ValidateObjectNullability(rootObj, item, vrl);
        }
        else
        {
            if (mustNotBeNull)
            {
                var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;
                var label = rootObj.GetType().GetDisplayNameForProperty(prefix);
                vrl.AddValidationResult(new ValidationResult($"The {label} field is required.", [prefix]));
            }
        }
    }
    #endregion 
}