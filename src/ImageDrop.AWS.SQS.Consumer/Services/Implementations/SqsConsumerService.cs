namespace ImageDrop.AWS.SQS.Consumer.Services.Implementations;

internal sealed class SqsConsumerService(
    IAmazonSQS amazonSqs, 
    IMessageDispatcher messageDispatcher, 
    IOptions<SqsOptions> options, 
    ILogger<SqsConsumerService> logger) : ISqsConsumerService
{
    private readonly SqsOptions _sqsOptions = options.Value;
    
    public async Task PollAndHandleMessages(CancellationToken cancellationToken)
    {
        GetQueueUrlResponse queueUrlResponse = await amazonSqs.GetQueueUrlAsync(_sqsOptions.ImageDropQueueName, cancellationToken);

        ReceiveMessageRequest receiveMessageRequest = new()
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            MessageAttributeNames = _sqsOptions.Consumer.MessageAttributeNames,
            WaitTimeSeconds = _sqsOptions.Consumer.WaitTimeSeconds,
            VisibilityTimeout = _sqsOptions.Consumer.VisibilityTimeout,
            MessageSystemAttributeNames = _sqsOptions.Consumer.MessageSystemAttributeNames,
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                ReceiveMessageResponse messageResponse =
                    await amazonSqs.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);

                if (messageResponse.HttpStatusCode is not HttpStatusCode.OK)
                {
                    logger.LogError("Queue {QueueName} responded with {HttpStatusCode}", _sqsOptions.ImageDropQueueName, messageResponse.HttpStatusCode);
                    
                    continue;
                }

                foreach (Message message in messageResponse.Messages ?? [])
                {
                    await ProcessMessage(message, cancellationToken);
                    await amazonSqs.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, $"Service {nameof(SqsConsumerService)} iteration failed.");
            }
        }
    }

    private async Task ProcessMessage(Message message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.Body))
        {
            logger.LogError("Received empty message {MessageMessageId}.", message.MessageId);
            
            return;
        }

        string? messageTypeName = message.MessageAttributes.GetValueOrDefault(nameof(IReceiveMessage.MessageTypeName))?.StringValue;

        if (messageTypeName is null)
        {
            logger.LogError("Message {MessageId} is missing message type attribute.", message.MessageId);
            
            return;
        }

        if (!messageDispatcher.CanHandleMessageType(messageTypeName))
        {
            logger.LogError("Cannot handle message type  {MessageType} for message {MessageId}.", messageTypeName, message.MessageId);
            
            return;
        }
        
        Type? messageType = messageDispatcher.GetMessageTypeByName(messageTypeName);

        if (messageType is null)
        {
            logger.LogError("Unknown message type {MessageType} for message {MessageId}.", messageTypeName, message.MessageId);
            
            return;
        }

        IReceiveMessage? decerializedMessage = messageDispatcher.GetMessageAsType(message, messageType);

        if (decerializedMessage is null)
        {
            logger.LogError("Cannot deserialize message {MessageId}. It will be removed from queue.", message.MessageId);
            
            return;
        }
        
        bool dispatchSucceeded = await messageDispatcher.Dispatch(decerializedMessage);

        if (!dispatchSucceeded)
        {
            logger.LogError("Cannot dispatch message {MessageId}.", message.MessageId);
            
            return;
        }
        
        logger.LogInformation("Message {MessageId} successfully processed.", message.MessageId);
    }
}