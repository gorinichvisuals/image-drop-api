namespace ImageDrop.Shared.Constants.Options;

public sealed class S3Options
{
    public required string PublicBaseUrl { get; init; }
    public required string BucketName { get; init; }
}