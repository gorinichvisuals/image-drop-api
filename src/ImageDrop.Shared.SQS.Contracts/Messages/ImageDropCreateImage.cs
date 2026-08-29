namespace ImageDrop.Shared.SQS.Contracts.Messages;

public class ImageDropCreateImage : IReceiveMessage
{
    [JsonPropertyName("imageId")]
    public required Guid ImageId { get; set; }
    
    [JsonIgnore]
    public string MessageTypeName => nameof(ImageDropCreateImage);
    [JsonIgnore]
    public string MessageId { get; set; } = string.Empty;
    [JsonIgnore]
    public DateTimeOffset SentAt { get; set; }
}