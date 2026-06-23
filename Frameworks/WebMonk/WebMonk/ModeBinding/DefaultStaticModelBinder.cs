using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Supermodel.DataAnnotations;
using Supermodel.DataAnnotations.Attributes;
using Supermodel.DataAnnotations.Validations;
using WebMonk.Context;
using WebMonk.Exceptions;
using WebMonk.Extensions;
using WebMonk.ValueProviders;

namespace WebMonk.ModeBinding;

public class DefaultStaticModelBinder : IStaticModelBinder
{
    #region IStaticModelBinder implementation
    public Task<object?> BindNewModelAsync(Type rootType, Type modelType, List<IValueProvider> valueProviders, bool ignoreRootObjectIModelBinder = false)
    {
        return BindExistingModelAsync(rootType, modelType, Type.Missing, valueProviders, ignoreRootObjectIModelBinder);
    }
    public async Task<object?> BindExistingModelAsync(Type rootType, Type modelType, object? model, List<IValueProvider> valueProviders, bool ignoreRootObjectISelfModelBinder = false)
    {
        var prefix = HttpContext.Current.PrefixManager.CurrentPrefix;
        var name = prefix.ToHtmlName();

        //types that self-ModelBind
        if (!ignoreRootObjectISelfModelBinder && typeof(ISelfModelBinder).IsAssignableFrom(modelType)) model = await BindToIModelBinderObjectAsync(rootType, modelType, model, valueProviders).ConfigureAwait(false);

        //string
        else if (modelType == typeof(string)) model = valueProviders.GetValueOrDefault<string>(name).UpdateInternal(model);
            
        //integer types
        else if (modelType == typeof(int)) model = BindToSimpleType<int>(rootType, model, valueProviders, name);
        else if (modelType == typeof(int?)) model = BindToSimpleType<int?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(uint)) model = BindToSimpleType<uint>(rootType, model, valueProviders, name);
        else if (modelType == typeof(uint?)) model = BindToSimpleType<uint?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(long)) model = BindToSimpleType<long>(rootType, model, valueProviders, name);
        else if (modelType == typeof(long?)) model = BindToSimpleType<long?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(ulong)) model = BindToSimpleType<ulong>(rootType, model, valueProviders, name);
        else if (modelType == typeof(ulong?)) model = BindToSimpleType<ulong?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(short)) model = BindToSimpleType<short>(rootType, model, valueProviders, name);
        else if (modelType == typeof(short?)) model = BindToSimpleType<short?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(ushort)) model = BindToSimpleType<ushort>(rootType, model, valueProviders, name);
        else if (modelType == typeof(ushort?)) model = BindToSimpleType<ushort?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(byte)) model = BindToSimpleType<byte>(rootType, model, valueProviders, name);
        else if (modelType == typeof(byte?)) model = BindToSimpleType<byte?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(sbyte)) model = BindToSimpleType<sbyte>(rootType, model, valueProviders, name);
        else if (modelType == typeof(sbyte?)) model = BindToSimpleType<sbyte?>(rootType, model, valueProviders, name);

        //floating point types
        else if (modelType == typeof(double)) model = BindToSimpleType<double>(rootType, model, valueProviders, name);
        else if (modelType == typeof(double?)) model = BindToSimpleType<double?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(float)) model = BindToSimpleType<float>(rootType, model, valueProviders, name);
        else if (modelType == typeof(float?)) model = BindToSimpleType<float?>(rootType, model, valueProviders, name);
        else if (modelType == typeof(decimal)) model = BindToSimpleType<decimal>(rootType, model, valueProviders, name);
        else if (modelType == typeof(decimal?)) model = BindToSimpleType<decimal?>(rootType, model, valueProviders, name);

        //booleans
        else if (modelType == typeof(bool)) model = BindToSimpleType<bool>(rootType, model, valueProviders, name);
        else if (modelType == typeof(bool?)) model = BindToSimpleType<bool?>(rootType, model, valueProviders, name);
            
        //DateTime
        else if (modelType == typeof(DateTime)) model = BindToSimpleType<DateTime>(rootType, model, valueProviders, name);
        else if (modelType == typeof(DateTime?)) model = BindToSimpleType<DateTime?>(rootType, model, valueProviders, name);
            
        //Enums
        else if (modelType.IsEnum || Nullable.GetUnderlyingType(modelType)?.IsEnum == true) model = BindToSimpleType(rootType, modelType, model, valueProviders, name);

        //Guids
        else if (modelType == typeof(Guid)) model = BindToSimpleType<Guid>(rootType, model, valueProviders, name);
        else if (modelType == typeof(Guid?)) model = BindToSimpleType<Guid?>(rootType, model, valueProviders, name);

        //Binary data
        else if (modelType == typeof(byte[])) model = BindToSimpleType<byte[]?>(rootType, model, valueProviders, name);

        //Arrays, Lists, and Dictionaries
        else if (typeof(IEnumerable).IsAssignableFrom(modelType) && modelType != typeof(string)) model = await BindToIEnumerableAsync(rootType, modelType, model, name, valueProviders).ConfigureAwait(false);

        //Complex objects
        else if (modelType.IsComplexType()) model = await BindToComplexObjectAsync(rootType, modelType, model, valueProviders).ConfigureAwait(false);

        //Non-supported type
        else throw new WebMonkException($"Unable to model bind to type {modelType.Name}");

        return model;
    }
    #endregion

    #region Helper Methods
    protected object? BindToSimpleType<T>(Type rootType, object? model, List<IValueProvider> valueProviders, string name)
    {
        return BindToSimpleType(rootType, typeof(T), model, valueProviders, name);
    }
    protected object? BindToSimpleType(Type rootType, Type modelType, object? model, List<IValueProvider> valueProviders, string name)
    {
        try
        {
            return valueProviders.GetValueOrDefault(name, modelType).UpdateInternal(model);
        }
        catch (WebMonkInvalidFormatException)
        {
            var label = rootType.GetDisplayNameForProperty(name);
            HttpContext.Current.ValidationResultList.Add(new ValidationResult($"Invalid format for {label}", [name]));
            return Type.Missing;
        }
    }
        
    protected async Task<object?> BindToIModelBinderObjectAsync(Type rootType, Type modelType, object? model, List<IValueProvider> valueProviders)
    {
        if (model == null || model == Type.Missing) model = Activator.CreateInstance(modelType);
        if (model is IAsyncInit iAsyncInit) await iAsyncInit.InitAsync().ConfigureAwait(false);

        if (model is ISelfModelBinder modelBinder) model = await modelBinder.BindMeAsync(rootType, valueProviders).ConfigureAwait(false);
        else throw new WebException("This should never happen: model is not IModelBinder modelBinder");
        return model;
    }
    protected async Task<object?> BindToIEnumerableAsync(Type rootType, Type modelType, object? model, string name, List<IValueProvider> valueProviders)
    {
        var indexesWithValue = valueProviders.GetIndexesWithValue(name);
        if (indexesWithValue != null)
        {
            // ReSharper disable once RedundantAssignment
            if (modelType.IsArray)
            {
                var intIndexesWithValue = indexesWithValue.Select(int.Parse).ToList();
                var length = intIndexesWithValue.Max() + 1;
                var innerType = modelType.GetElementType();
                var modelArr = Array.CreateInstance(modelType.GetElementType()!, length);
                foreach (var index in intIndexesWithValue)
                {
                    using(HttpContext.Current.PrefixManager.NewPrefix($"[{index}]", modelArr))
                    {
                        var newValue = await BindNewModelAsync(rootType, innerType!, valueProviders).ConfigureAwait(false);
                        if (newValue != null && newValue != Type.Missing) 
                        {
                            var vrl = new ValidationResultList();
                            if (!await AsyncValidator.TryValidateObjectAsync(newValue, new ValidationContext(newValue), vrl).ConfigureAwait(false))
                            {
                                HttpContext.Current.ValidationResultList.AddValidationResultList(vrl, $"{HttpContext.Current.PrefixManager.CurrentPrefix}");
                            }
                        }
                        if (newValue != Type.Missing) modelArr.SetValue(newValue, index);       
                    }
                }
                model = modelArr;
            }
            else if (modelType.IsGenericType && typeof(IList).IsAssignableFrom(modelType))
            {
                if (model == null || model == Type.Missing) model = Activator.CreateInstance(modelType);
                if (model is IAsyncInit iAsyncInit) await iAsyncInit.InitAsync().ConfigureAwait(false);

                var modelList = (IList)model;
                    
                var intIndexesWithValue = indexesWithValue.Select(int.Parse).ToList();
                var length = intIndexesWithValue.Max() + 1;
                var innerType = modelType.GenericTypeArguments[0];
                modelList.Clear();
                for(var i = 0; i < length; i++) 
                {
                    object? innerObj;
                    if (innerType.IsValueType)
                    {
                        innerObj = Activator.CreateInstance(innerType);
                        if (innerObj is IAsyncInit iInnerAsyncInit) await iInnerAsyncInit.InitAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        innerObj = null;
                    }
                    modelList.Add(innerObj);
                }
                foreach (var index in intIndexesWithValue)
                {
                    using(HttpContext.Current.PrefixManager.NewPrefix($"[{index}]", modelList))
                    {
                        var newValue = await BindNewModelAsync(rootType, innerType, valueProviders).ConfigureAwait(false);
                        if (newValue != null && newValue != Type.Missing) 
                        {
                            var vrl = new ValidationResultList();
                            if (!await AsyncValidator.TryValidateObjectAsync(newValue, new ValidationContext(newValue), vrl).ConfigureAwait(false))
                            {
                                HttpContext.Current.ValidationResultList.AddValidationResultList(vrl, $"{HttpContext.Current.PrefixManager.CurrentPrefix}");
                            }
                        }
                        if (newValue != Type.Missing) modelList[index] = newValue;
                    }
                }
            }
            else if (modelType.IsGenericType && typeof(IDictionary).IsAssignableFrom(modelType))
            {
                if (model == null || model == Type.Missing) model = Activator.CreateInstance(modelType);
                if (model is IAsyncInit iAsyncInit) await iAsyncInit.InitAsync().ConfigureAwait(false);
                var modelDict = (IDictionary)model;
                    
                modelDict.Clear();
                var innerType = modelType.GenericTypeArguments[1];
                foreach (var index in indexesWithValue)
                {
                    using(HttpContext.Current.PrefixManager.NewPrefix($"[{index}]", modelDict))
                    {
                        var newValue = await BindNewModelAsync(rootType, innerType, valueProviders).ConfigureAwait(false);
                        if (newValue != null && newValue != Type.Missing) 
                        {
                            var vrl = new ValidationResultList();
                            if (!await AsyncValidator.TryValidateObjectAsync(newValue, new ValidationContext(newValue), vrl).ConfigureAwait(false))
                            {
                                HttpContext.Current.ValidationResultList.AddValidationResultList(vrl, $"{HttpContext.Current.PrefixManager.CurrentPrefix}");
                            }
                        }
                        if (newValue != Type.Missing) modelDict[index] = newValue;        
                    }
                }
            }
            else
            {
                throw new WebMonkException($"Unable to ModelBind to {modelType.Name} type, can only bind to generic IEnumerable types such as Arrays, Lists, and Dictionaries");
            }
        }
        return model;
    }
    protected async Task<object?> BindToComplexObjectAsync(Type rootType, Type modelType, object? model, List<IValueProvider> valueProviders)
    {
        if (model == null || model == Type.Missing) model = Activator.CreateInstance(modelType);
        if (model is IAsyncInit iAsyncInit) await iAsyncInit.InitAsync().ConfigureAwait(false);

        foreach (var propertyInfo in GetPropertiesForModelBinding(modelType))
        {
            using(HttpContext.Current.PrefixManager.NewPrefix(propertyInfo.Name, model))
            {
                // ReSharper disable once RedundantCast
                object? propertyValue = model != null && model != Type.Missing ? propertyInfo.GetValue(model) : Type.Missing;
                propertyValue = await BindExistingModelAsync(rootType, propertyInfo.PropertyType, propertyValue, valueProviders).ConfigureAwait(false);
                if (propertyValue != null && propertyValue != Type.Missing) 
                {
                    var vrl = new ValidationResultList();
                    if (!await AsyncValidator.TryValidateObjectAsync(propertyValue, new ValidationContext(propertyValue), vrl).ConfigureAwait(false))
                    {
                        HttpContext.Current.ValidationResultList.AddValidationResultList(vrl, HttpContext.Current.PrefixManager.CurrentPrefix);
                    }
                }
                if (propertyValue != Type.Missing) propertyInfo.SetValue(model, propertyValue);
            }
        }
        return model;
    }
        
    protected virtual IEnumerable<PropertyInfo> GetPropertiesForModelBinding(Type type)
    {
        var properties = type.GetProperties()
            .Where(x => x.GetCustomAttribute<DoNotBindAttribute>() == null && 
                        x.SetMethod != null && x.SetMethod.IsPublic && 
                        x.GetMethod != null && x.GetMethod.IsPublic);

        return properties;
    }
    #endregion
}