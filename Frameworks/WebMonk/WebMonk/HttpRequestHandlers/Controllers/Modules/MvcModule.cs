namespace WebMonk.HttpRequestHandlers.Controllers.Modules;

public class MvcModule : MvcController
{
    #region Overrides
    //mvc and api modules take precedence over controllers
    public override int Priority => 250;
    #endregion
}