namespace ImageDrop.AWS.SQS.Publisher.Services;

internal sealed class SqsPublisher(IAmazonSQS sqsClient, IOptions<SqsOptions> options, ILogger<SqsPublisher> logger) : ISqsPublisher
{
    private readonly string _imageDropQueueName = options.Value.ImageDropQueueName;
    
    public async Task PublishToImageDropQueue<TMessage>(TMessage message, string messageGroupId) where TMessage : IMessage
        => await PublishAsync(_imageDropQueueName, message, messageGroupId);
    
    private async Task PublishAsync<TMessage>(string queueName, TMessage message, string messageGroupId) where TMessage : IMessage
    {
        GetQueueUrlResponse queueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);
        string body = JsonSerializer.Serialize(message);
        
        SendMessageRequest request = new()
        {
            QueueUrl = queueUrlResponse.QueueUrl,
            MessageBody = body,
            MessageGroupId = messageGroupId,
            MessageAttributes = InitMessageAttibutes(message),
            MessageDeduplicationId = Guid.CreateVersion7().ToString()
        };
        
        await sqsClient.SendMessageAsync(request);
    }

    private static Dictionary<string, MessageAttributeValue> InitMessageAttibutes<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        Dictionary<string, MessageAttributeValue> values = new()
        { 
            { 
                nameof(IMessage.MessageTypeName), new MessageAttributeValue()
                {
                    StringValue = message.MessageTypeName,
                    DataType = nameof(String),
                }   
            } 
        };
        
        return values;
    }
}