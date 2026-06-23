using System;

namespace WebMonk.HttpRequestHandlers.Controllers;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class EndPointAttribute : Attribute
{
    #region Constructors
    public EndPointAttribute(string url, bool exclusive = true)
    {
        Url = url;
        Exclusive = exclusive;
    }
    #endregion

    #region Properties
    public string Url { get; }
    public bool Exclusive { get; }
    #endregion
}