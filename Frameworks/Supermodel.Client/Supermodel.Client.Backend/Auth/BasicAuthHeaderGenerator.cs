using Supermodel.Encryptor;

namespace Supermodel.Client.Backend.Auth;

public class BasicAuthHeaderGenerator : IAuthHeaderGenerator
{
    #region Constructors
    public BasicAuthHeaderGenerator(string username, string password)
    {
        Username = username;
        Password = password;
    }
    #endregion
				
    #region Methods
    public virtual AuthHeader CreateAuthHeader()
    {
        return HttpAuthAgent.CreateBasicAuthHeader(Username, Password);
    }
    #endregion
				
    #region Properties
    public long? UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    #endregion
}