using System.Net.Mail;

namespace BeagleLib.Util;

//Noblis Setup
public static class BConfig
{
    public static string Version => "v1.72";
    public static bool EmailEnabled => false;

    public static string SystemEmail => @"""Beagle"" <beagle@noblis.org>";
    public static string ToEmail => "ilya.basin@noblis.org"; //@"2628939741@txt.att.net";

    public static string SMTPServer => "smtp.noblis.org";
    //public static string SMTPServer => "mail.edge.noblis.org";
    public static int SMTPPort => 25;
    public static string SMTPUsername => "";
    public static string SMTPPassword => "";

    public static bool EnableSsl => false;
    public static SmtpDeliveryMethod DeliveryMethod => SmtpDeliveryMethod.Network;

    #region Constants
    public const int StackSize = 32;
    public const int ClipboardSize = 32;

    public const int MaxScore = 10000;
    public const int MaxScriptLength = 320;

    public const int TopMostAccurateOrganismsToKeep = 10;
    #endregion
}