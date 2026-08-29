namespace ImageDrop.AWS.SQS.Consumer.Helper;

public interface IImageDropHelper
{
    IReadOnlyDictionary<string, Type> GetMessageMappings();
    IReadOnlyDictionary<string, Func<IServiceProvider, IMessageHandler>> GetHandlers();
}