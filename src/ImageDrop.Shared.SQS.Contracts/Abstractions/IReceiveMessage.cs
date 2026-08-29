namespace ImageDrop.Shared.SQS.Contracts.Abstractions;

public interface IReceiveMessage : IMessage
{
    public string MessageId { get; set; }
    public DateTimeOffset SentAt { get; set; }
}