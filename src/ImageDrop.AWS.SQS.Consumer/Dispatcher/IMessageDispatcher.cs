namespace ImageDrop.AWS.SQS.Consumer.Dispatcher;

public interface IMessageDispatcher
{
    Task<bool> Dispatch<TMessage>(TMessage message) where TMessage : IReceiveMessage;
    bool CanHandleMessageType(string messageTypeName);
    Type? GetMessageTypeByName(string messageTypeName);
    IReceiveMessage? GetMessageAsType( Message message, Type messageType);
}