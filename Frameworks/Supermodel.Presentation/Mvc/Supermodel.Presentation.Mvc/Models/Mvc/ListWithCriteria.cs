using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supermodel.DataAnnotations;
using Supermodel.DataAnnotations.Exceptions;
using Supermodel.ReflectionMapper;

namespace Supermodel.Presentation.Mvc.Models.Mvc;

public class ListWithCriteria<TListItem, TCriteria> : List<TListItem>, IRMapperCustom
{
    #region IRMapperCustom implementation
    public async Task MapFromCustomAsync<T>(T other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        //Check if we are mapping from a list with Criteria
        var propertyInfo = other.GetType().GetProperty("Criteria");
        if (propertyInfo != null && propertyInfo.PropertyType == typeof(TCriteria)) other.PropertySet("Criteria", Criteria);

        var otherIEnumerable = (IEnumerable)other;
        var myEnumerableInterfaceType = GetType().GetInterface(typeof(IEnumerable<>).Name);
        if (myEnumerableInterfaceType == null) throw new SupermodelException("enumerableInterfaceType == null");
        var myIEnumerableGenericArg = myEnumerableInterfaceType.GetGenericArguments()[0];
        foreach (var otherItemObj in otherIEnumerable)
        {
            var item = otherItemObj != null ? await ReflectionHelper.CreateType(myIEnumerableGenericArg).ExecuteGenericMethod("MapFromCustomAsync",
                [otherItemObj.GetType()], otherItemObj )!.GetResultAsObjectAsync() : null;
            if (item is IAsyncInit iAsyncInitItem && !iAsyncInitItem.AsyncInitialized) await iAsyncInitItem.InitAsync();
            Add((TListItem)item!); //this is ok if item is null
        }
    }

    public async Task<T> MapToCustomAsync<T>(T other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        //Check if we are mapping to a list with Criteria
        var propertyInfo = other.GetType().GetProperty("Criteria");
        if (propertyInfo != null && propertyInfo.PropertyType == typeof(TCriteria)) Criteria = (TCriteria)other.PropertyGet("Criteria")!;

        var myICollection = (ICollection)this;
        var otherICollection = (ICollection)other;

        var otherEnumerableInterfaceType = other.GetType().GetInterface(typeof(IEnumerable<>).Name);
        if (otherEnumerableInterfaceType == null) throw new SupermodelException("enumerableInterfaceType == null");
        var otherICollectionGenericArg = otherEnumerableInterfaceType.GetGenericArguments()[0];
        foreach (var myItemObj in myICollection)
        {
            // ReSharper disable once MergeConditionalExpression
            var item = myItemObj != null ? await myItemObj.ExecuteGenericMethod("MapToCustomAsync",
                [otherICollectionGenericArg], ReflectionHelper.CreateType(otherICollectionGenericArg))!.GetResultAsObjectAsync() : null;
            if (item is IAsyncInit iAsyncInitItem && !iAsyncInitItem.AsyncInitialized) await iAsyncInitItem.InitAsync();
            otherICollection.AddToCollection(item);
        }
            
        return other;
    }
    #endregion

    #region Properties
    // ReSharper disable once RedundantDefaultMemberInitializer
    [NotRMapped] public TCriteria Criteria { get; set; } = default!;
    #endregion
}