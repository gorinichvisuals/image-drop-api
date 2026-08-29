namespace ImageDrop.AWS.SQS.Consumer.Handlers;

internal sealed class ImageDropCreateImageHandler(
    IImageProcessingService imageProcessingService, 
    ILogger<ImageDropCreateImageHandler> logger) : IMessageHandler
{
    public static Type MessageType => typeof(ImageDropCreateImage);
    
    public async Task<bool> HandleAsync(IReceiveMessage message)
    {
        try
        {
            ImageDropCreateImage? imageMessage = message as ImageDropCreateImage;

            return imageMessage is not null && await imageProcessingService.ProcessImage(imageMessage.ImageId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error handling {MessageType} message.", message.MessageTypeName);
            
            return false;
        }
    }
}