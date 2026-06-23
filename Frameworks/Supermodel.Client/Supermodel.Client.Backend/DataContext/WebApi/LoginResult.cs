namespace Supermodel.Client.Backend.DataContext.WebApi;

public class LoginResult
{
    #region Contructors
    public LoginResult(bool loginSuccessful, long? userId, string? userLabel)
    {
        LoginSuccessful = loginSuccessful;
        if (loginSuccessful)
        {
            UserId = userId;
            UserLabel = userLabel;

        }
        else
        {
            UserId = null;
            UserLabel = null;
        }
    }
    #endregion

    #region Properties
    public bool LoginSuccessful { get; set; }
    public long? UserId { get; set; }
    public string? UserLabel { get; set; }
    #endregion
}