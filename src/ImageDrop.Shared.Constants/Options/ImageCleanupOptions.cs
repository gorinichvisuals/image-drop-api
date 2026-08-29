namespace ImageDrop.Shared.Constants.Options;

public sealed class ImageCleanupOptions
{
    public int UnownedImageRetentionDays { get; init; }
    public int MaxDeleteObjectsBatchSize { get; init; }
    public required string TriggerIdentity { get; init; }
    public int IntervalInHours  { get; init; }
}