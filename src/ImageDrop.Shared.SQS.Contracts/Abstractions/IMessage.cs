namespace ImageDrop.Shared.SQS.Contracts.Abstractions;

public interface IMessage
{
    public string MessageTypeName { get; }
}