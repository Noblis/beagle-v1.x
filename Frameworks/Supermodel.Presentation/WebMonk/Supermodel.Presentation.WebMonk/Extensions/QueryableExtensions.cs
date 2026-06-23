using System;
using System.Linq;
using System.Linq.Expressions;
using Supermodel.DataAnnotations.Exceptions;

namespace Supermodel.Presentation.WebMonk.Extensions;

public static class QueryableExtensions
{
    #region Dynamic Sort methods
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> queryableSource, string property)
    {
        return queryableSource.ApplyOrder(property, "OrderBy");
    }
    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> queryableSource, string property)
    {
        return queryableSource.ApplyOrder(property, "OrderByDescending");
    }
        
    public static IOrderedQueryable<T> ThenOrderBy<T>(this IQueryable<T> queryableSource, string property)
    {
        return queryableSource.ApplyOrder(property, "ThenBy");
    }
    public static IOrderedQueryable<T> ThenOrderByDescending<T>(this IQueryable<T> queryableSource, string property)
    {
        return queryableSource.ApplyOrder(property, "ThenByDescending");
    }
        
    private static IOrderedQueryable<T> ApplyOrder<T>(this IQueryable<T> queryableSource, string property, string methodName)
    {
        var props = property.Split('.');
        var type = typeof(T);
        var arg = Expression.Parameter(type, "x");
        var expr = (Expression)arg;
        foreach (var prop in props)
        {
            var propertyInfo = type.GetProperty(prop);
            if (propertyInfo == null) throw new ArgumentException(nameof(property));
            // ReSharper disable once AssignNullToNotNullAttribute
            expr = Expression.Property(expr, propertyInfo);
            type = propertyInfo.PropertyType;
        }
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        var lambda = Expression.Lambda(delegateType, expr, arg);

        var result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), type)
            .Invoke(null, [queryableSource, lambda]);
        if (result == null) throw new SupermodelException("result == null");
        return (IOrderedQueryable<T>)result;
    } 
    #endregion
}