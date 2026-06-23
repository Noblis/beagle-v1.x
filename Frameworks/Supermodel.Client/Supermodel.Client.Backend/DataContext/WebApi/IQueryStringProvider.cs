namespace Supermodel.Client.Backend.DataContext.WebApi;

public interface IQueryStringProvider
{
    string GetQueryString(object? searchBy, int? skip, int? take, string? sortBy);
}