namespace ImageDrop.Shared.Constants.Options;

public sealed class AWSCoreOptions
{
    public RegionEndpoint Region { get; init; } = RegionEndpoint.EUWest1;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
}