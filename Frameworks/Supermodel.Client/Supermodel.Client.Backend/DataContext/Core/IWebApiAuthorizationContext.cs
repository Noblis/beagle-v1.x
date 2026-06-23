using Supermodel.Client.Backend.DataContext.WebApi;
using Supermodel.Client.Backend.Models;
using Supermodel.Encryptor;

namespace Supermodel.Client.Backend.DataContext.Core;

public interface IWebApiAuthorizationContext
{
    AuthHeader? AuthHeader { get; set; }
    Task<LoginResult> ValidateLoginAsync<TModel>() where TModel : class, IModel;
}