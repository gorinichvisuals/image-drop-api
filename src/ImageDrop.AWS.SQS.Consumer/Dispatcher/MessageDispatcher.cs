namespace ImageDrop.AWS.SQS.Consumer.Dispatcher;

internal sealed class MessageDispatcher(
    IServiceScopeFactory scopeFactory, 
    IImageDropHelper imageDropHelper, 
    ILogger<MessageDispatcher> logger) : IMessageDispatcher
{    
    private readonly IReadOnlyDictionary<string, Type> _messageMappings =
        imageDropHelper.GetMessageMappings();

    private readonly IReadOnlyDictionary<string, Func<IServiceProvider, IMessageHandler>> _handlers =
        imageDropHelper.GetHandlers();

    public async Task<bool> Dispatch<TMessage>(TMessage message) where TMessage : IReceiveMessage
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        if (!_handlers.TryGetValue(
                message.MessageTypeName,
                out Func<IServiceProvider, IMessageHandler>? handlerFactory))
        {
            logger.LogError(
                "Handler for message type {MessageType} was not found.",
                message.MessageTypeName);

            return false;
        }

        IMessageHandler handler = handlerFactory(scope.ServiceProvider);

        return await handler.HandleAsync(message);
    }

    public bool CanHandleMessageType(string messageTypeName)
        => _handlers.ContainsKey(messageTypeName);

    public Type? GetMessageTypeByName(string messageTypeName)
        => _messageMappings.GetValueOrDefault(messageTypeName);

    public IReceiveMessage? GetMessageAsType(Message message, Type messageType)
    {
        try
        {
            if (JsonSerializer.Deserialize(message.Body, messageType) is not IReceiveMessage messageAsType)
                return null;

            messageAsType.MessageId = message.MessageId;

            if (message.Attributes.TryGetValue(MessageSystemAttributeName.SentTimestamp, out string? timestamp))
                messageAsType.SentAt =
                    DateTimeOffset.FromUnixTimeMilliseconds(
                        long.Parse(timestamp));

            return messageAsType;
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Failed to deserialize message {MessageId} as {MessageType}.", 
                message.MessageId, messageType.Name);

            return null;
        }
    }
}