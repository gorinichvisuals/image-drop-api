namespace ImageDrop.AWS.SQS.Consumer.Services.Abstractions;

public interface ISqsConsumerService
{
    Task PollAndHandleMessages(CancellationToken cancellationToken);
}