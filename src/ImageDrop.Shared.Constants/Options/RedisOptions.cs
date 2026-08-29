namespace ImageDrop.Shared.Constants.Options;

public sealed class RedisOptions
{
    public required string Configuration { get; init; }
    public required string InstanceName { get; init; }
}