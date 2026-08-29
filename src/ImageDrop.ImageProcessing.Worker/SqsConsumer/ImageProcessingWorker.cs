namespace ImageDrop.ImageProcessing.Worker.SqsConsumer;

internal class ImageProcessingWorker(
    ISqsConsumerService sqsConsumerService, 
    ILogger<ImageProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await sqsConsumerService.PollAndHandleMessages(stoppingToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Worker {nameof(ImageProcessingWorker)} threw an unhandled exception.");
        }
    }
}