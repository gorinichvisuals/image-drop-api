namespace ImageDrop.AWS.SQS.Consumer.Handlers;

public interface IMessageHandler
{
    static Type? MessageType { get; }
    
    Task<bool> HandleAsync(IReceiveMessage message);
}