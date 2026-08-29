namespace ImageDrop.AWS.SQS.Consumer.Helper;

internal sealed class ImageDropHelper : IImageDropHelper
{    
    private static readonly Assembly MessageAssembly =
        typeof(IMessage).Assembly;

    private static readonly Assembly[] HandlerAssemblies =
    [
        typeof(IMessageHandler).Assembly
    ];
    
    public IReadOnlyDictionary<string, Type> GetMessageMappings()
    {
        return MessageAssembly
            .DefinedTypes
            .Where(type =>
                typeof(IMessage).IsAssignableFrom(type) &&
                !type.IsInterface &&
                !type.IsAbstract)
            .ToDictionary(
                type => type.Name,
                type => type.AsType());
    }

    public IReadOnlyDictionary<string, Func<IServiceProvider, IMessageHandler>> GetHandlers()
    {
        return HandlerAssemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type =>
                typeof(IMessageHandler).IsAssignableFrom(type) &&
                !type.IsInterface &&
                !type.IsAbstract)
            .ToDictionary<TypeInfo, string, Func<IServiceProvider, IMessageHandler>>(
                type =>
                    ((Type)type
                        .GetProperty(nameof(IMessageHandler.MessageType))!
                        .GetValue(null)!)!
                    .Name,
                type => serviceProvider =>
                    (IMessageHandler)serviceProvider.GetRequiredService(type.AsType()));
    }
}