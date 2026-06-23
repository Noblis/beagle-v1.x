#nullable disable

using System.Globalization;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Supermodel.Client.Backend.Multipart;

public static class FormattingUtilities
{
    // Supported date formats for input.
    private static readonly string[] _dateFormats =
    {
        // "r", // RFC 1123, required output format but too strict for input
        "ddd, d MMM yyyy H:m:s 'GMT'", // RFC 1123 (r, except it allows both 1 and 01 for date and time)
        "ddd, d MMM yyyy H:m:s", // RFC 1123, no zone - assume GMT
        "d MMM yyyy H:m:s 'GMT'", // RFC 1123, no day-of-week
        "d MMM yyyy H:m:s", // RFC 1123, no day-of-week, no zone
        "ddd, d MMM yy H:m:s 'GMT'", // RFC 1123, short year
        "ddd, d MMM yy H:m:s", // RFC 1123, short year, no zone
        "d MMM yy H:m:s 'GMT'", // RFC 1123, no day-of-week, short year
        "d MMM yy H:m:s", // RFC 1123, no day-of-week, short year, no zone

        "dddd, d'-'MMM'-'yy H:m:s 'GMT'", // RFC 850
        "dddd, d'-'MMM'-'yy H:m:s", // RFC 850 no zone
        "ddd MMM d H:m:s yyyy", // ANSI C's asctime() format

        "ddd, d MMM yyyy H:m:s zzz", // RFC 5322
        "ddd, d MMM yyyy H:m:s", // RFC 5322 no zone
        "d MMM yyyy H:m:s zzz", // RFC 5322 no day-of-week
        "d MMM yyyy H:m:s" // RFC 5322 no day-of-week, no zone
    };

    // Valid header token characters are within the range 0x20 < c < 0x7F excluding the following characters
    private const string NonTokenChars = "()<>@,;:\\\"/[]?={}";

    public const double Match = 1.0;
    public const double NoMatch = 0.0;
    public const int DefaultMaxDepth = 256;
    public const int DefaultMinDepth = 1;
    public const string HttpRequestedWithHeader = @"x-requested-with";
    public const string HttpRequestedWithHeaderValue = @"XMLHttpRequest";
    public const string HttpHostHeader = "Host";
    public const string HttpVersionToken = "HTTP";
    public static readonly Type HttpRequestMessageType = typeof(HttpRequestMessage);
    public static readonly Type HttpResponseMessageType = typeof(HttpResponseMessage);
    public static readonly Type HttpContentType = typeof(HttpContent);
    //public static readonly Type DelegatingEnumerableGenericType = typeof(DelegatingEnumerable<>);
    public static readonly Type EnumerableInterfaceGenericType = typeof(IEnumerable<>);
    public static readonly Type QueryableInterfaceGenericType = typeof(IQueryable<>);
    public static bool IsJTokenType(Type type)
    {
        return typeof(JToken).IsAssignableFrom(type);
    }
    public static HttpContentHeaders CreateEmptyContentHeaders()
    {
        HttpContent tempContent = null;
        HttpContentHeaders contentHeaders;
        try
        {
            tempContent = new StringContent(string.Empty);
            contentHeaders = tempContent.Headers;
            contentHeaders.Clear();
        }
        finally
        {
            // We can dispose the content without touching the headers
            if (tempContent != null) tempContent.Dispose();
        }

        return contentHeaders;
    }
    public static string UnquoteToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return token;
        if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1) return token.Substring(1, token.Length - 2);
        return token;
    }

    public static bool ValidateHeaderToken(string token)
    {
        if (token == null) return false;
        foreach (var c in token)
        {
            if (c < 0x21 || c > 0x7E || NonTokenChars.IndexOf(c) != -1) return false;
        }
        return true;
    }

    public static string DateToString(DateTimeOffset dateTime)
    {
        // Format according to RFC1123; 'r' uses invariant info (DateTimeFormatInfo.InvariantInfo)
        return dateTime.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture);
    }

    public static bool TryParseDate(string input, out DateTimeOffset result)
    {
        return DateTimeOffset.TryParseExact(input, _dateFormats, DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
            out result);
    }

    public static bool TryParseInt32(string value, out int result)
    {
        return int.TryParse(value, NumberStyles.None, NumberFormatInfo.InvariantInfo, out result);
    }    
}