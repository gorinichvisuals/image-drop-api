namespace ImageDrop.Application.Services.Abstractions;

public interface ICacheService
{
    Task<(bool ItemExists, T? Value)> TryGet<T>(string key);
    Task Set<T>(string key, T value, TimeSpan? absoluteExpiration = null);
}