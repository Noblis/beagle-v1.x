using WebMonk.Exceptions;

namespace WebMonk.HttpRequestHandlers.Controllers.Modules;

public class ApiModule : ApiController
{
    #region Constructors
    public ApiModule()
    {
        //verify module name is valid
        var myTypeName = GetType().Name;
        if (!myTypeName.EndsWith("ApiModule")) throw new WebMonkException("Api Modules must end with ApiModule");
    }
    #endregion

    #region Overrides
    //mvc and api modules take precedence over controllers
    public override int Priority => 250;
    #endregion
}