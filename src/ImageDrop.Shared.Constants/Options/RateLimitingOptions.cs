namespace ImageDrop.Shared.Constants.Options;

public sealed class RateLimitingOptions
{
    public int IpPermitLimit { get; init; }
    public int IpWindowMinutes { get; init; }
}