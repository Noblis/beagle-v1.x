namespace Supermodel.Client.Backend.DataContext.Core;

public interface ICachedDataContext
{
    int CacheAgeToleranceInSeconds { get; set; }
    Task PurgeCacheAsync(int? cacheExpirationAgeInSeconds = null, Type? modelType = null);
}