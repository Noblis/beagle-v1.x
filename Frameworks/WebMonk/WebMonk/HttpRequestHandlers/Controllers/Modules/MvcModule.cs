using WebMonk.Exceptions;

namespace WebMonk.HttpRequestHandlers.Controllers.Modules;

public class MvcModule : MvcController
{
    #region Constructors
    public MvcModule()
    {
        //verify module name is valid
        var myTypeName = GetType().Name;
        if (!myTypeName.EndsWith("MvcModule")) throw new WebMonkException("Mvc Modules must end with MvcModule");
    }
    #endregion

    #region Overrides
    //mvc and api modules take precedence over controllers
    public override int Priority => 250;
    #endregion
}