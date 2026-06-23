using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Supermodel.DataAnnotations;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.DataAnnotations.Validations;

namespace Supermodel.ReflectionMapper;

public static class RMExtensions
{
    #region Methods
    #nullable disable
    public static async Task<TMe> MapFromAsync<TMe, TPassive>(this TMe me, TPassive passive, bool forceShallowForAllProps = false)
    {
        var myType = me.GetType();
            
        if (me is IRMapperCustom customMe)
        {
            var otherType = typeof(TPassive);
            if (passive != null && !(otherType.IsGenericType && otherType.GetGenericTypeDefinition() == typeof(Nullable<>))) otherType = passive.GetType();

            if (typeof(TPassive) == otherType)
            {
                await customMe.MapFromCustomAsync(passive);
            }
            else
            {
                var otherObj = (object)passive;
                me = (TMe)await me.MapFromAsync(otherObj, otherType, forceShallowForAllProps); //don't need myType.IsMarkedForShallowCopyFrom() because it is done inside 
            }
        }
        else
        {
            me = await MapFromCustomBaseAsync(me, passive, forceShallowForAllProps || myType.IsMarkedForShallowCopyFrom());
        }
        return me;
    }
    public static async Task<TPassive> MapToAsync<TMe, TPassive>(this TMe me, TPassive passive, bool forceShallowForAllProps = false)
    {
        var myType = me.GetType();

        if (me is IRMapperCustom customMe)
        {
            var otherType = typeof(TPassive);
            if (passive != null && !(otherType.IsGenericType && otherType.GetGenericTypeDefinition() == typeof(Nullable<>))) otherType = passive.GetType();

            // ReSharper disable once PossibleNullReferenceException
            if (otherType.FullName.StartsWith("Castle.Proxies.")) otherType = otherType.BaseType ?? throw new SupermodelException("Castle.Proxies does not have a base type.");

            if (typeof(TPassive) == otherType)
            {
                passive = await customMe.MapToCustomAsync(passive);
            }
            else
            {
                var otherObj = (object)passive;
                passive = (TPassive)await me.MapToAsync(otherObj, otherType, forceShallowForAllProps); //don't need myType.IsMarkedForShallowCopyTo() because it is done inside
            }
        }
        else
        {
            passive = await MapToCustomBaseAsync(me, passive, forceShallowForAllProps || myType.IsMarkedForShallowCopyTo());
        }
        return passive;
    }

    public static Task<object> MapFromAsync(this object me, object passive, Type passiveType, bool forceShallowForAllProps = false)
    {
        var myType = me.GetType();
        var task = ReflectionHelper.ExecuteStaticGenericMethod(typeof(RMExtensions), nameof(MapFromAsync), [myType, passiveType
        ], me, passive, forceShallowForAllProps || myType.IsMarkedForShallowCopyFrom());
        if (task == null) throw new SystemException("MapFromAsync: task == null");
        return task.GetResultAsObjectAsync();
    }
    public static Task<object> MapToAsync(this object me, object passive, Type passiveType, bool forceShallowForAllProps = false)
    {
        var myType = me.GetType();
        var task = ReflectionHelper.ExecuteStaticGenericMethod(typeof(RMExtensions), nameof(MapToAsync), [myType, passiveType
        ], me, passive, forceShallowForAllProps || myType.IsMarkedForShallowCopyTo());
        if (task == null) throw new SystemException("MapToAsync: task == null");
        return task.GetResultAsObjectAsync();
    }

    public static async Task<TMe> MapFromCustomBaseAsync<TMe, TPassive>(this TMe me, TPassive passive, bool forceShallowForAllProps = false)
    {
        if (me == null) throw new ReflectionMapperException($"{nameof(MapFromCustomBaseAsync)}: me is null");
        if (passive == null) throw new ReflectionMapperException($"{nameof(MapFromCustomBaseAsync)}: passive is null");

        var myType = me.GetType();
        var otherType = passive.GetType();

        //if primitive types
        if (myType.IsPrimitiveOrValueTypeOrNullable())
        {
            //if primitive type, it must match 100%
            if (myType == otherType) return (TMe)(object)passive;
            else throw new PropertyCantBeAutomappedException($"Cannot map {myType} to {otherType} because their types are incompatible.");
        }

        //Arrays with same element types of active element type that implements ICustomMapper
        if (AreCompatibleArrays(myType, otherType))
        {
            var otherArray = (Array)(object)passive;
            var myArray = (Array)(object)me;
            var myArrayItemType = myArray.GetType().GetElementType();
            if (myArrayItemType == null) throw new SupermodelException("myArrayItemType == null");
            var otherArrayItemType = otherArray.GetType().GetElementType();

            myArray = Array.CreateInstance(myArrayItemType, otherArray.Length);

            for (var i = 0; i < otherArray.Length; i++)
            {
                var otherArrayItem = otherArray.GetValue(i);
                if (otherArrayItem == null)
                {
                    myArray.SetValue(null, i);
                }
                else
                {
                    if (forceShallowForAllProps)
                    {
                        myArray.SetValue(otherArrayItem, i);
                    }
                    else
                    {
                        var myArrayItem = ReflectionHelper.CreateType(myArrayItemType);
                        if (myArrayItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);
                        await myArrayItem.MapFromAsync(otherArrayItem, otherArrayItemType);
                        myArray.SetValue(myArrayItem, i);
                    }
                }
            }
            return (TMe)(object)myArray;
        }

        //ICollection<ICustomMapper> (if main objects are compatible collections)
        //WARNING: if we derive from a collection, we ignore all properties that could be on a collection object itself
        if (AreCompatibleCollections(myType, otherType))
        {
            var otherICollection = (ICollection)passive;
            var myICollection = (ICollection)me;
            var myICollectionItemType = myType.GetInterfaces().Single(x => x.Name == typeof(IEnumerable<>).Name).GetGenericArguments()[0];
            var otherICollectionItemType = otherType.GetTypeInfo().ImplementedInterfaces.Single(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];

            myICollection.ClearCollection();
            foreach (var otherICollectionItem in otherICollection)
            {
                if (otherICollectionItem == null)
                {
                    myICollection.AddToCollection(null);
                }
                else
                {
                    if (forceShallowForAllProps)
                    {
                        myICollection.AddToCollection(otherICollectionItem);
                    }
                    else
                    {
                        var myICollectionItem = ReflectionHelper.CreateType(myICollectionItemType);
                        if (myICollectionItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        await myICollectionItem.MapFromAsync(otherICollectionItem, otherICollectionItemType);
                        myICollection.AddToCollection(myICollectionItem);
                    }
                }
            }
            return me;
        }

        foreach (var myPropertyInfo in myType.GetProperties())
        {
            //Get my property meta
            var myPropertyMeta = new RMPropertyMetadata(me, myPropertyInfo);

            //If property is marked NotRMappedAttribute, don't worry about this one
            if (myPropertyMeta.IsMarkedNotRMappedForMappingFrom()) continue;

            //Find matching passive property
            var otherPropertyMeta = GetMatchingProperty(me, myPropertyMeta.PropertyInfo, passive);

            //Get passive prop value 
            var otherProperty = otherPropertyMeta.Get();

            //Get my property value
            var myProperty = myPropertyMeta.Get();

            //ICustomMapper. This is intentionally not "is" because myProperty could be null and we still use ICustomMapper
            if (typeof(IRMapperCustom).IsAssignableFrom(myPropertyMeta.PropertyInfo.PropertyType))
            {
                try
                {
                    if (myProperty == null)
                    {
                        myProperty = ReflectionHelper.CreateType(myPropertyMeta.PropertyInfo.PropertyType);
                        if (myProperty is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        myPropertyMeta.Set(myProperty, true);
                    }

                    if (myProperty is IRMapperCustom myPropertyRMCustom)
                    {
                        //the below is equivalent of await myPropertyRMCustom.MapFromCustomAsync(otherProperty); but we need passive property to go in as <T>
                        var task = myPropertyRMCustom.ExecuteGenericMethod(nameof(IRMapperCustom.MapFromCustomAsync),
                            [otherPropertyMeta.PropertyInfo.PropertyType], otherProperty);  
                        if (task == null) throw new SystemException("MapFromCustomAsync: task == null");
                        await task.GetResultAsObjectAsync();

                    }
                    else
                    {
                        throw new ReflectionMapperException($"{nameof(MapFromCustomBaseAsync)}: myProperty does not implement {nameof(IRMapperCustom)}. This should never happen");
                    }
                }
                catch (ValidationResultException ex)
                {
                    var vr = new ValidationResultList();
                    foreach (var validationResult in ex.ValidationResultList)
                    {
                        if (validationResult.MemberNames.Any(x => !string.IsNullOrWhiteSpace(x))) vr.Add(validationResult);
                        else vr.Add(new ValidationResult(validationResult.ErrorMessage, [myPropertyMeta.PropertyInfo.Name
                        ]));
                    }
                    throw new ValidationResultException(vr);
                }
                catch (Exception ex)
                {
                    throw new PropertyCantBeAutomappedException($"Property '{myPropertyMeta.PropertyInfo.Name}' of class '{myType.Name}' can't be automapped to type '{otherType.Name}' property '{otherPropertyMeta.PropertyInfo.Name}' because ICustomMapper implementation threw an exception: {ex.Message}.");
                }
            }

            //If passive prop value is null, set my prop to null and done
            else if (otherProperty == null)
            {
                myPropertyMeta.Set(null, true);
            }

            //if properties are compatible arrays
            else if (!forceShallowForAllProps && !myPropertyMeta.IsMarkedForShallowCopyFrom() && AreCompatibleArrays(myPropertyMeta.PropertyInfo.PropertyType, otherPropertyMeta.PropertyInfo.PropertyType))
            {
                var otherArray = (Array)otherProperty;
                var myArrayItemType = myPropertyMeta.PropertyInfo.PropertyType.GetElementType();
                if (myArrayItemType == null) throw new SupermodelException("myArrayItemType == null");
                var otherArrayItemType = otherPropertyMeta.PropertyInfo.PropertyType.GetElementType();

                //if (myProperty == null) myProperty = myPropertyMeta.CreateDefaultInstance();
                var myArray = Array.CreateInstance(myArrayItemType, otherArray.Length);

                for (var i = 0; i < otherArray.Length; i++)
                {
                    var otherArrayItem = otherArray.GetValue(i);
                    if (otherArrayItem == null)
                    {
                        myArray.SetValue(null, i);
                    }
                    else
                    {
                        var myArrayItem = ReflectionHelper.CreateType(myArrayItemType);
                        if (myArrayItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        myArrayItem = await myArrayItem.MapFromAsync(otherArrayItem, otherArrayItemType);
                        myArray.SetValue(myArrayItem, i);
                    }
                }
                myPropertyMeta.Set(myArray, true);
            }

            //ICollection<ICustomMapper> (if properties are compatible collections)
            else if (!forceShallowForAllProps && !myPropertyMeta.IsMarkedForShallowCopyFrom() && AreCompatibleCollections(myPropertyMeta.PropertyInfo.PropertyType, otherPropertyMeta.PropertyInfo.PropertyType))
            {
                var otherICollection = (ICollection)otherProperty;
                var myICollectionItemType = myPropertyMeta.PropertyInfo.PropertyType.GetTypeInfo().ImplementedInterfaces.Single(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];
                var otherICollectionItemType = otherPropertyMeta.PropertyInfo.PropertyType.GetTypeInfo().ImplementedInterfaces.Single(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];

                myProperty ??= myPropertyMeta.CreateDefaultInstance();
                var myICollection = (ICollection)myProperty;
                myICollection!.ClearCollection();

                foreach (var otherICollectionItem in otherICollection)
                {
                    if (otherICollectionItem == null)
                    {
                        myICollection.AddToCollection(null);
                    }
                    else
                    {
                        var myICollectionItem = ReflectionHelper.CreateType(myICollectionItemType);
                        if (myICollectionItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        await myICollectionItem.MapFromAsync(otherICollectionItem, otherICollectionItemType);
                        myICollection.AddToCollection(myICollectionItem);
                    }
                }
                myPropertyMeta.Set(myICollection, true);
            }

            else if (myPropertyMeta.PropertyInfo.PropertyType.IsPrimitiveOrValueTypeOrNullable())
            {
                //if primitive type, it must match 100%
                if (myPropertyMeta.PropertyInfo.PropertyType == otherPropertyMeta.PropertyInfo.PropertyType) myPropertyMeta.Set(otherProperty, true);
                else throw new PropertyCantBeAutomappedException($"Property '{myPropertyMeta.PropertyInfo.Name}' of class '{myType.Name}' can't be automapped to type '{otherType.Name}' property '{otherPropertyMeta.PropertyInfo.Name}' because their types are incompatible.");
            }

            else
            {
                //if non-primitive type...
                if (forceShallowForAllProps || myPropertyMeta.IsMarkedForShallowCopyFrom())
                {
                    //... and marked for shallow copy, types must be assignable
                    if (!myPropertyMeta.PropertyInfo.PropertyType.IsAssignableFrom(otherPropertyMeta.PropertyInfo.PropertyType))
                    {
                        throw new PropertyCantBeAutomappedException($"Property '{myPropertyMeta.PropertyInfo.Name}' of class '{myType.Name}' can't be automapped to type '{otherType.Name}' property '{otherPropertyMeta.PropertyInfo.Name}' because it is marked for shallow copy and their types are incompatible.");
                    }
                    myPropertyMeta.Set(otherProperty, true);
                }
                else
                {
                    //... and not marked for shallow copy, use reflection mapper recursively
                    myProperty ??= myPropertyMeta.CreateDefaultInstance();
                    await myProperty.MapFromAsync(otherProperty, otherPropertyMeta.PropertyInfo.PropertyType);
                    myPropertyMeta.Set(myProperty, true);
                }
            }
        }
        return me;
    }
    public static async Task<TPassive> MapToCustomBaseAsync<TMe, TPassive>(this TMe me, TPassive passive, bool forceShallowForAllProps = false)
    {
        if (me == null) throw new ReflectionMapperException($"{nameof(MapToCustomBaseAsync)}: me is null");
        if (passive == null) throw new ReflectionMapperException($"{nameof(MapToCustomBaseAsync)}: passive is null");

        var myType = me.GetType();
        var otherType = passive.GetType();

        //if primitive types
        if (myType.IsPrimitiveOrValueTypeOrNullable())
        {
            //if primitive type, it must match 100%
            if (myType == otherType) return (TPassive)(object)me;
            else throw new PropertyCantBeAutomappedException($"Cannot map {myType} to {otherType} because their types are incompatible.");
        }

        //Arrays with same element types of active element type that implements ICustomMapper
        if (AreCompatibleArrays(myType, otherType))
        {
            var myArray = (Array)(object)me;
            var otherArray = (Array)(object)passive;
            var otherArrayItemType = otherArray.GetType().GetElementType();
            if (otherArrayItemType == null) throw new SupermodelException("otherArrayItemType == null");

            otherArray = Array.CreateInstance(otherArrayItemType, myArray.Length);
            for (var i = 0; i < myArray.Length; i++)
            {
                var myArrayItem = myArray.GetValue(i);
                if (myArrayItem == null)
                {
                    otherArray.SetValue(null, i);
                }
                else
                {
                    if (forceShallowForAllProps)
                    {
                        otherArray.SetValue(myArrayItem, i);
                    }
                    else
                    {
                        var otherArrayItem = ReflectionHelper.CreateType(otherArrayItemType);
                        if (otherArrayItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        otherArrayItem = await myArrayItem.MapToAsync(otherArrayItem, otherArrayItemType);
                        otherArray.SetValue(otherArrayItem, i);
                    }
                }
            }
            return (TPassive)(object)otherArray;
        }

        //ICollection<ICustomMapper> (if main objects are compatible collections)
        //WARNING: if we derive from a collection, we ignore all properties that could be on a collection object itself
        if (AreCompatibleCollections(myType, otherType))
        {
            var myICollection = (ICollection)me;
            var otherICollection = (ICollection)passive;
            var otherICollectionItemType = otherType.GetTypeInfo().ImplementedInterfaces.Single(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];

            otherICollection.ClearCollection();
            foreach (var myICollectionItem in myICollection)
            {
                if (myICollectionItem == null)
                {
                    otherICollection.AddToCollection(null);
                }
                else
                {
                    if (forceShallowForAllProps)
                    {
                        otherICollection.AddToCollection(myICollectionItem);
                    }
                    else
                    {
                        var otherICollectionItem = ReflectionHelper.CreateType(otherICollectionItemType);
                        if (otherICollectionItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        otherICollectionItem = await myICollectionItem.MapToAsync(otherICollectionItem, otherICollectionItemType);
                        otherICollection.AddToCollection(otherICollectionItem);
                    }
                }
            }
            return passive;
        }

        foreach (var myPropertyInfo in myType.GetProperties())
        {
            //Get my property meta
            var myPropertyMeta = new RMPropertyMetadata(me, myPropertyInfo);

            //If property is marked NotRMappedAttribute, don't worry about this one
            if (myPropertyMeta.IsMarkedNotRMappedForMappingTo()) continue;

            //Find matching property on passive
            var otherPropertyMeta = GetMatchingProperty(me, myPropertyMeta.PropertyInfo, passive);

            //Get my property value
            var myProperty = myPropertyMeta.Get();

            //Get passive property
            var otherProperty = otherPropertyMeta.Get();

            //ICustomMapper. Here we use "is" because if myProperty is null we handle it in the next "else if"
            if (myProperty is IRMapperCustom myPropertyRMCustom)
            {
                try
                {
                    //the below is equivalent of otherProperty = await myPropertyRMCustom.MapToCustomAsync(otherProperty); but we need passive property to go in as <T>
                    var task = myPropertyRMCustom.ExecuteGenericMethod(nameof(IRMapperCustom.MapToCustomAsync),
                        [otherPropertyMeta.PropertyInfo.PropertyType], otherProperty);
                    if (task == null) throw new SystemException("MapToCustomAsync: task == null");
                    otherProperty = await task.GetResultAsObjectAsync();
                    otherPropertyMeta.Set(otherProperty, true);
                }
                catch (ValidationResultException ex)
                {
                    var vr = new ValidationResultList();
                    foreach (var validationResult in ex.ValidationResultList)
                    {
                        if (validationResult.MemberNames.Any(x => !string.IsNullOrWhiteSpace(x))) vr.Add(validationResult);
                        else vr.Add(new ValidationResult(validationResult.ErrorMessage, [myPropertyMeta.PropertyInfo.Name
                        ]));
                    }
                    throw new ValidationResultException(vr);
                }
                catch (Exception ex)
                {
                    throw new PropertyCantBeAutomappedException($"Property '{myPropertyMeta.PropertyInfo.Name}' of class '{myType.Name}' can't be automapped to type '{otherType.Name}' property '{otherPropertyMeta.PropertyInfo.Name}' because IRMapperCustom threw an exception: {ex.Message}.");
                }
            }

            //if my property is null, set passive property to null and done
            else if (myProperty == null)
            {
                otherPropertyMeta.Set(null, true);
            }

            //if properties are compatible arrays
            else if (!forceShallowForAllProps && !myPropertyMeta.IsMarkedForShallowCopyTo() && AreCompatibleArrays(myPropertyMeta.PropertyInfo.PropertyType, otherPropertyMeta.PropertyInfo.PropertyType))
            {
                var myArray = (Array)myProperty;
                var otherArrayItemType = otherPropertyMeta.PropertyInfo.PropertyType.GetElementType();
                if (otherArrayItemType == null) throw new SupermodelException("otherArrayItemType == null");
                var otherArray = Array.CreateInstance(otherArrayItemType, myArray.Length);

                for (var i = 0; i < myArray.Length; i++)
                {
                    var myArrayItem = myArray.GetValue(i);
                    if (myArrayItem == null)
                    {
                        otherArray.SetValue(null, i);
                    }
                    else
                    {
                        var otherArrayItem = ReflectionHelper.CreateType(otherArrayItemType);
                        if (otherArrayItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        otherArrayItem = await myArrayItem.MapToAsync(otherArrayItem, otherArrayItemType);
                        otherArray.SetValue(otherArrayItem, i);
                    }
                }
                otherPropertyMeta.Set(otherArray, true);
            }

            //ICollection<ICustomMapper>
            else if (!forceShallowForAllProps && !myPropertyMeta.IsMarkedForShallowCopyTo() && AreCompatibleCollections(myPropertyMeta.PropertyInfo.PropertyType, otherPropertyMeta.PropertyInfo.PropertyType))
            {
                var myICollection = (IEnumerable)myProperty;
                var otherICollectionItemType = otherPropertyMeta.PropertyInfo.PropertyType.GetTypeInfo().ImplementedInterfaces.Single(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments()[0];

                otherProperty ??= otherPropertyMeta.CreateDefaultInstance();
                var otherICollection = (ICollection)otherProperty;

                otherICollection.ClearCollection();
                foreach (var myICollectionItem in myICollection)
                {
                    if (myICollectionItem == null)
                    {
                        otherICollection.AddToCollection(null);
                    }
                    else
                    {
                        var otherICollectionItem = ReflectionHelper.CreateType(otherICollectionItemType);
                        if (otherICollectionItem is IAsyncInit iAsyncInit && !iAsyncInit.AsyncInitialized) await iAsyncInit.InitAsync().ConfigureAwait(false);

                        otherICollectionItem = await myICollectionItem.MapToAsync(otherICollectionItem, otherICollectionItemType);
                        otherICollection.AddToCollection(otherICollectionItem);
                    }
                }
                otherPropertyMeta.Set(otherICollection, true);
            }

            else if (myPropertyMeta.PropertyInfo.PropertyType.IsPrimitiveOrValueTypeOrNullable())
            {
                //if primitive type, it must match 100%
                if (myPropertyMeta.PropertyInfo.PropertyType == otherPropertyMeta.PropertyInfo.PropertyType) otherPropertyMeta.Set(myProperty, true);
                else throw new PropertyCantBeAutomappedException($"Property '{myPropertyMeta.PropertyInfo.Name}' of class '{myType.Name}' can't be automapped to type '{otherType.Name}' property '{otherPropertyMeta.PropertyInfo.Name}' because their types are incompatible.");
            }

            else
            {
                //if non-primitive type...
                if (forceShallowForAllProps || myPropertyMeta.IsMarkedForShallowCopyTo())
                {
                    //... and marked for shallow copy, types must be assignable
                    if (!otherPropertyMeta.PropertyInfo.PropertyType.IsAssignableFrom(myPropertyMeta.PropertyInfo.PropertyType))
                    {
                        throw new PropertyCantBeAutomappedException($"Property '{myPropertyMeta.PropertyInfo.Name}' of class '{myType.Name}' can't be automapped to type '{otherType.Name}' property '{otherPropertyMeta.PropertyInfo.Name}' because it is marked for shallow copy and their types are incompatible.");
                    }
                    otherPropertyMeta.Set(myProperty, true);
                }
                else
                {
                    //... and not marked for shallow copy, use reflection mapper recursively
                    otherProperty ??= otherPropertyMeta.CreateDefaultInstance();

                    //Because this is a non-primitive type, we don't need to assign the result of the method below
                    await myProperty.MapToAsync(otherProperty, otherPropertyMeta.PropertyInfo.PropertyType);

                    otherPropertyMeta.Set(otherProperty, true);
                }
            }
        }

        return passive;
    }
    #nullable enable

    public static bool IsEqualToObject(this object me, object? other)
    {
        //Handle nulls
        if (other == null) return false;

        var myType = me.GetType();
        var otherType = other.GetType();

        //We only compare identical types or when obj is derived from me (this is needed for EF support)
        if (!ReflectionHelper.IsClassADerivedFromClassB(otherType, myType)) throw new ArgumentException($"Cannot compare incompatible types {myType.Name} and {otherType.Name}");

        //IComparable
        if (me is IComparable myIComparable)
        {
            try
            {
                return myIComparable.CompareTo(other) == 0;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"{myType.Name} can't be compared to {otherType.Name} because IComparable implementation threw an exception: {ex.Message}.");
            }
        }

        //IEnumerable
        else if (me is IEnumerable myIEnumerable)
        {
            var otherIEnumerable = (IEnumerable)other;

            var otherIEnumerableEnumerator = otherIEnumerable.GetEnumerator();
            using var otherIEnumerableEnumeratorDisposer = otherIEnumerableEnumerator as IDisposable;

            var myIEnumerableEnumerator = myIEnumerable.GetEnumerator();
            using var myIEnumerableEnumeratorDisposer = myIEnumerableEnumerator as IDisposable;

            while (true)
            {
                var otherEnumeratorDone = !otherIEnumerableEnumerator.MoveNext();
                var myEnumeratorDone = !myIEnumerableEnumerator.MoveNext();

                //if both are done and we have not found a difference, return true
                if (otherEnumeratorDone && myEnumeratorDone) return true;

                //If both are not done but one is done, enumerators are of different length, return false
                if (otherEnumeratorDone || myEnumeratorDone) return false;

                var otherItem = otherIEnumerableEnumerator.Current;
                var myItem = myIEnumerableEnumerator.Current;

                if (!myItem!.IsEqualToObject(otherItem)) return false;
            }
        }

        //Go through all properties that are at least readable
        foreach (var myPropertyInfo in myType.GetProperties().Where(x => x.CanRead))
        {
            //If property is marked NotRComparedAttribute, don't worry about this one
            if (myPropertyInfo.GetCustomAttribute(typeof(NotRComparedAttribute), true) != null) continue;

            //Find matching properties
            var otherPropertyInfo = otherType.GetProperty(myPropertyInfo.Name);
            if (otherPropertyInfo == null) throw new ReflectionMapperException($"{nameof(IsEqualToObject)}: otherPropertyInfo == null. This should never happen");

            //Get the property values
            var otherProperty = other.PropertyGet(otherPropertyInfo.Name);
            var myProperty = me.PropertyGet(myPropertyInfo.Name);

            //Handle nulls
            if (myProperty == null)
            {
                if (otherProperty == null) continue;
                else return false;
            }

            //IComparer
            if (myProperty is IComparable myPropertyObjAsComparable)
            {
                try
                {
                    if (myPropertyObjAsComparable.CompareTo(otherProperty) != 0) return false;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Property '{myPropertyInfo.Name}' of type '{myType.Name}' can't be compared to type '{otherType.Name}' property '{otherPropertyInfo.Name}' because IComparable implementation threw an exception: {ex.Message}.");
                }
            }

            //Everything else
            else
            {
                try
                {
                    if (!myProperty.IsEqualToObject(otherProperty)) return false;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Property '{myPropertyInfo.Name}' of type '{myType.Name}' can't be compared to type '{otherType.Name}' property '{otherPropertyInfo.Name}' because IsEqualToObject() method threw an exception: {ex.Message}.");
                }
            }
        }
        return true;
    }
    public static bool IsEnumOrNullableEnum(this Type me)
    {
        if (me.IsEnum) return true;
        if (me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>) && me.GenericTypeArguments[0].IsEnum) return true;
        return false;
    }
    public static bool IsPrimitiveOrValueTypeOrNullable(this Type me)
    {
        if (me.IsPrimitive || me.IsValueType || me == typeof(string)) return true;
        return me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsMarkedForShallowCopyFrom(this Type me)
    {
        return me.HasAttribute<RMCopyAllPropsShallowAttribute>() || me.HasAttribute<RMCopyAllPropsShallowFromAttribute>();   
    }
    public static bool IsMarkedForShallowCopyTo(this Type me)
    {
        return me.HasAttribute<RMCopyAllPropsShallowAttribute>() || me.HasAttribute<RMCopyAllPropsShallowToAttribute>();
    }
    #endregion

    #region Private Helper Methods
    private static bool AreCompatibleArrays(Type activeType, Type passiveType)
    {
        //both types are arrays
        if (!activeType.GetTypeInfo().IsArray) return false;
        if (!passiveType.GetTypeInfo().IsArray) return false;

        //element types match - don't need this
        //var activeElementType = activeType.GetElementType();
        //var passiveElementType = passiveType.GetElementType();
        //if (activeElementType != passiveElementType) return false;

        return true;
    }
    private static bool AreCompatibleCollections(Type activeType, Type passiveType)
    {
        //both types are generic
        if (!activeType.GetTypeInfo().IsGenericType) return false;
        if (!passiveType.GetTypeInfo().IsGenericType) return false;

        //get generic type definitions
        var activeTypeGenericDefinition = activeType.GetGenericTypeDefinition();
        var passiveTypeGenericDefinition = passiveType.GetGenericTypeDefinition();

        //make sure the types are the same except for the generic parameter
        if (activeTypeGenericDefinition != passiveTypeGenericDefinition) return false;

        //set up ICollection interface in a variable
        //var activeICollectionInterface = activeType.GetInterface(typeof(ICollection<>).Name);
        var activeICollectionInterface = activeType.GetTypeInfo().ImplementedInterfaces.SingleOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
        if (activeICollectionInterface == null && activeTypeGenericDefinition == typeof(ICollection<>)) activeICollectionInterface = activeType;

        //var passiveICollectionInterface = passiveType.GetInterface(typeof(ICollection<>).Name);
        var passiveICollectionInterface = passiveType.GetTypeInfo().ImplementedInterfaces.SingleOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
        if (passiveICollectionInterface == null && passiveTypeGenericDefinition == typeof(ICollection<>)) passiveICollectionInterface = passiveType;

        //both types are ICollection
        if (activeICollectionInterface == null) return false;
        if (passiveICollectionInterface == null) return false;

        //both are generic interfaces (this might be generic but just in case)
        if (!activeICollectionInterface.GetTypeInfo().IsGenericType) return false;
        if (!passiveICollectionInterface.GetTypeInfo().IsGenericType) return false;

        //element types match - don't need this
        //var activeElementType = activeICollectionInterface.GetGenericArguments()[0];
        //var passiveElementType = passiveICollectionInterface.GetGenericArguments()[0];
        //if (activeElementType != passiveElementType) return false;

        return true;
    }
    private static RMPropertyMetadata GetMatchingProperty(object me, PropertyInfo myProperty, object other)
    {
        // ReSharper disable PossibleMultipleEnumeration
        // ReSharper disable AccessToForEachVariableInClosure

        var myPropertyName = myProperty.Name;
        var parentObj = other;

        var myReflectionMappedToAttribute = myProperty.GetCustomAttribute(typeof(RMapsToAttribute), true);
        if (myReflectionMappedToAttribute != null)
        {
            var attrPropertyName = ((RMapsToAttribute)myReflectionMappedToAttribute).PropertyName;
            if (!string.IsNullOrEmpty(attrPropertyName)) myPropertyName = attrPropertyName;

            var attrObjectPath = ((RMapsToAttribute)myReflectionMappedToAttribute).ObjectPath;
            if (!string.IsNullOrEmpty(attrObjectPath))
            {
                var mappedToNameComponents = attrObjectPath.Split('.');
                if (mappedToNameComponents.Length < 1) throw new ReflectionMapperException($"Invalid path in {nameof(RMapsToAttribute)}");

                foreach (var subPropertyName in mappedToNameComponents)
                {
                    var subObjProperties = parentObj.GetType().GetProperties().Where(x => x.Name == subPropertyName);
                    if (subObjProperties.Count() != 1) throw new ReflectionMapperException($"Invalid path in {nameof(RMapsToAttribute)}");
                    parentObj = parentObj.PropertyGet(subObjProperties.Single().Name) ?? throw new SystemException("parentObj.PropertyGet(subObjProperties.Single().Name) == null");
                    //if (parentObj == null) throw new ReflectionMapperException($"{subPropertyName} in {nameof(RMapToAttribute)} Attribute path is null");
                }
            }
        }

        var otherProperties = parentObj.GetType().GetProperties().Where(x => x.Name == myPropertyName);
        if (otherProperties.Count() != 1) throw new PropertyCantBeAutomappedException($"Property '{myProperty.Name}' of class '{me.GetType().Name}' can't be automapped to type '{parentObj.GetType().Name}' because '{myPropertyName}' property does not exist in type '{parentObj.GetType().Name}'.");
        var propertyInfo = otherProperties.Single();

        return new RMPropertyMetadata(parentObj, propertyInfo);

        // ReSharper restore AccessToForEachVariableInClosure
        // ReSharper restore PossibleMultipleEnumeration
    }
    #endregion
}