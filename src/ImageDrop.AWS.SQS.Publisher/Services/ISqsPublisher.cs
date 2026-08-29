namespace ImageDrop.AWS.SQS.Publisher.Services;

public interface ISqsPublisher
{
    Task PublishToImageDropQueue<TMessage>(TMessage message, string messageGroupId) where TMessage : IMessage;
}