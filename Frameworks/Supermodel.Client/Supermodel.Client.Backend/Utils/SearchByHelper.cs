using System.Net;
using System.Reflection;
using System.Text;

namespace Supermodel.Client.Backend.Utils;

public static class SearchByHelper
{
    public static string ToQueryString<T>(this T me)
    {
        if (me == null) throw new NullReferenceException(nameof(me));
        
        var sb = new StringBuilder();

        var firstColumn = true;
        foreach (var property in me.GetType().GetTypeInfo().DeclaredProperties)
        {
            if (firstColumn) firstColumn = false;
            else sb.Append("&");

            var propertyObj = me.GetType().GetProperty(property.Name)?.GetValue(me);
            var propertyValue = "";
            if (propertyObj != null) propertyValue = propertyObj.ToString();

            sb.Append($"{property.Name}={WebUtility.UrlEncode(propertyValue)}");
        }
        return sb.ToString();
    }
}