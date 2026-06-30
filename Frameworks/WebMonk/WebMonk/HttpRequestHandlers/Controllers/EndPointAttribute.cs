using System;

namespace WebMonk.HttpRequestHandlers.Controllers;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class EndPointAttribute : Attribute
{
    #region Constructors
    public EndPointAttribute(string url)
    {
        Url = url;
    }
    #endregion

    #region Properties
    public string Url { get; }
    #endregion
}