using System.Globalization;
using System.Text.RegularExpressions;

namespace Supermodel.Client.Backend.Misc;

public class RegexHelper
{
    // ReSharper disable RedundantDefaultFieldInitializer
    private bool _invalid = false;
    // ReSharper restore RedundantDefaultFieldInitializer

    public bool IsValidEmail(string strIn)
    {
        _invalid = false;
        if (string.IsNullOrEmpty(strIn)) return false;

        // Use IdnMapping class to convert Unicode domain names.
        strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper);
        if (_invalid) return false;

        // Return true if strIn is in valid e-mail format.
        return Regex.IsMatch(strIn, EmailRegex, RegexOptions.IgnoreCase);
    }

    public static string EmailRegex => @"^(?("")(""[^""]+?""@)|(([0-9A-Za-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9A-Za-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9A-Za-z][-\w]*[0-9A-Za-z]*\.)+[a-zA-Z0-9]{2,17}))$";

    private string DomainMapper(Match match)
    {
        // IdnMapping class with default property values.
        var idn = new IdnMapping();

        var domainName = match.Groups[2].Value;
        try
        {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException)
        {
            _invalid = true;
        }
        return match.Groups[1].Value + domainName;
    }
}