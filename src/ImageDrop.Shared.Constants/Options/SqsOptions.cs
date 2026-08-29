namespace ImageDrop.Shared.Constants.Options;

public sealed class SqsOptions
{
    public required string ImageDropQueueName { get; init; }
    public required SqsConsumerOptions Consumer { get; init; }
}

public sealed class SqsConsumerOptions
{
    public int WaitTimeSeconds { get; init; }
    public int VisibilityTimeout { get; init; }
    public List<string> MessageAttributeNames { get; init; } = [];
    public List<string> MessageSystemAttributeNames { get; init; } = [];
}