using Supermodel.Encryptor;

namespace Supermodel.Client.Backend.Auth;

public interface IAuthHeaderGenerator
{
    long? UserId { get; set; }
    AuthHeader CreateAuthHeader();
}