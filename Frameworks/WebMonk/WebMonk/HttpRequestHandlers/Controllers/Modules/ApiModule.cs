namespace WebMonk.HttpRequestHandlers.Controllers.Modules;

public class ApiModule : ApiController
{
    #region Overrides
    //mvc and api modules take precedence over controllers
    public override int Priority => 250;
    #endregion
}