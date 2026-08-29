namespace ImageDrop.Application.Services.Implementations;

internal sealed class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<(bool ItemExists, T? Value)> TryGet<T>(string key)
    {
        string? cachedData = await cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cachedData))
            return (false, default);

        try
        {
            T? value = JsonSerializer.Deserialize<T>(cachedData);

            return (true, value);
        }
        catch
        {
            return (false, default);
        }
    }

    public async Task Set<T>(string key, T value, TimeSpan? absoluteExpiration = null)
    {
        DistributedCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromDays(1),
        };

        string? serialized = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, serialized, options, CancellationToken.None);
    }
}