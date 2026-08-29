namespace ImageDrop.AWS.SQS.Consumer.Tests.Unit.Dispatcher;

public sealed class MessageDispatcherTests
{
    private readonly IServiceScopeFactory _scopeFactoryMock;
    private readonly IImageDropHelper _imageDropHelperMock;
    private readonly ILogger<MessageDispatcher> _loggerMock;

    private readonly IServiceScope _scopeMock;
    private readonly IServiceProvider _serviceProviderMock;
    private readonly IMessageHandler _handlerMock;

    public MessageDispatcherTests()
    {
        _scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        _imageDropHelperMock = Substitute.For<IImageDropHelper>();
        _loggerMock = Substitute.For<ILogger<MessageDispatcher>>();

        _scopeMock = Substitute.For<IServiceScope>();
        _serviceProviderMock = Substitute.For<IServiceProvider>();
        _handlerMock = Substitute.For<IMessageHandler>();

        _scopeMock.ServiceProvider.Returns(_serviceProviderMock);
        _scopeFactoryMock.CreateScope().Returns(_scopeMock);
    }

    private MessageDispatcher CreateDispatcher(
        IReadOnlyDictionary<string, Type>? messageMappings = null,
        IReadOnlyDictionary<string, Func<IServiceProvider, IMessageHandler>>? handlers = null)
    {
        _imageDropHelperMock
            .GetMessageMappings()
            .Returns(messageMappings ?? new Dictionary<string, Type>());

        _imageDropHelperMock
            .GetHandlers()
            .Returns(handlers ??
                     new Dictionary<string, Func<IServiceProvider, IMessageHandler>>());

        return new MessageDispatcher(
            _scopeFactoryMock,
            _imageDropHelperMock,
            _loggerMock);
    }

    #region Dispatch

    [Fact]
    public async Task Dispatch_ShouldReturnHandlerResult_WhenHandlerFound()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageDropCreateImage message = new()
        {
            ImageId = imageId
        };

        Func<IServiceProvider, IMessageHandler> handlerFactory =
            _ => _handlerMock;

        MessageDispatcher dispatcher = CreateDispatcher(
            handlers: new Dictionary<string, Func<IServiceProvider, IMessageHandler>>
            {
                [nameof(ImageDropCreateImage)] = handlerFactory
            });

        _handlerMock
            .HandleAsync(message)
            .Returns(true);

        // Act
        bool result = await dispatcher.Dispatch(message);

        // Assert
        result.ShouldBeTrue();

        _scopeFactoryMock.Received(1).CreateScope();

        await _handlerMock.Received(1)
            .HandleAsync(message);
    }

    [Fact]
    public async Task Dispatch_ShouldReturnFalse_WhenHandlerReturnsFalse()
    {
        // Arrange
        ImageDropCreateImage message = new()
        {
            ImageId = Guid.NewGuid()
        };

        MessageDispatcher dispatcher = CreateDispatcher(
            handlers: new Dictionary<string, Func<IServiceProvider, IMessageHandler>>
            {
                [nameof(ImageDropCreateImage)] = _ => _handlerMock
            });

        _handlerMock
            .HandleAsync(message)
            .Returns(false);

        // Act
        bool result = await dispatcher.Dispatch(message);

        // Assert
        result.ShouldBeFalse();

        await _handlerMock.Received(1)
            .HandleAsync(message);
    }

    [Fact]
    public async Task Dispatch_ShouldReturnFalseAndLogError_WhenHandlerNotFound()
    {
        // Arrange
        ImageDropCreateImage message = new()
        {
            ImageId = Guid.NewGuid()
        };

        MessageDispatcher dispatcher = CreateDispatcher();

        // Act
        bool result = await dispatcher.Dispatch(message);

        // Assert
        result.ShouldBeFalse();

        await _handlerMock.DidNotReceiveWithAnyArgs()
            .HandleAsync(default!);

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion

    #region CanHandleMessageType

    [Fact]
    public void CanHandleMessageType_ShouldReturnTrue_WhenHandlerExists()
    {
        // Arrange
        MessageDispatcher dispatcher = CreateDispatcher(
            handlers: new Dictionary<string, Func<IServiceProvider, IMessageHandler>>
            {
                [nameof(ImageDropCreateImage)] = _ => _handlerMock
            });

        // Act
        bool result = dispatcher.CanHandleMessageType(nameof(ImageDropCreateImage));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanHandleMessageType_ShouldReturnFalse_WhenHandlerDoesNotExist()
    {
        // Arrange
        MessageDispatcher dispatcher = CreateDispatcher();

        // Act
        bool result = dispatcher.CanHandleMessageType(nameof(ImageDropCreateImage));

        // Assert
        result.ShouldBeFalse();
    }

    #endregion
    
    #region GetMessageTypeByName
    
    [Fact]
    public void GetMessageTypeByName_ShouldReturnType_WhenMessageExists()
    {
        // Arrange
        MessageDispatcher dispatcher = CreateDispatcher(
            messageMappings: new Dictionary<string, Type>
            {
                [nameof(ImageDropCreateImage)] = typeof(ImageDropCreateImage)
            });

        // Act
        Type? result =
            dispatcher.GetMessageTypeByName(nameof(ImageDropCreateImage));

        // Assert
        result.ShouldBe(typeof(ImageDropCreateImage));
    }

    [Fact]
    public void GetMessageTypeByName_ShouldReturnNull_WhenMessageDoesNotExist()
    {
        // Arrange
        MessageDispatcher dispatcher = CreateDispatcher();

        // Act
        Type? result =
            dispatcher.GetMessageTypeByName(nameof(ImageDropCreateImage));

        // Assert
        result.ShouldBeNull();
    }
    
    #endregion
    
    #region GetMessageAsType
    
    [Fact]
    public void GetMessageAsType_ShouldDeserializeMessageAndSetMetadata()
    {
        // Arrange
        Guid messageId = Guid.NewGuid();
        DateTimeOffset sentAt = DateTimeOffset.UtcNow;

        ImageDropCreateImage sourceMessage = new()
        {
            ImageId = Guid.NewGuid()
        };

        Message message = new()
        {
            MessageId = messageId.ToString(),
            Body = JsonSerializer.Serialize(sourceMessage),
            Attributes = new Dictionary<string, string>
            {
                [MessageSystemAttributeName.SentTimestamp] =
                    sentAt.ToUnixTimeMilliseconds().ToString()
            }
        };

        MessageDispatcher dispatcher = CreateDispatcher();

        // Act
        IReceiveMessage? result =
            dispatcher.GetMessageAsType(
                message,
                typeof(ImageDropCreateImage));

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ImageDropCreateImage>();

        result.MessageId.ShouldBe(messageId.ToString());
    }
    
    [Fact]
    public void GetMessageAsType_ShouldReturnNullAndLogError_WhenJsonIsInvalid()
    {
        // Arrange
        Guid messageId = Guid.NewGuid();

        Message message = new()
        {
            MessageId = messageId.ToString(),
            Body = "{ invalid json",
            Attributes = new Dictionary<string, string>()
        };

        MessageDispatcher dispatcher = CreateDispatcher();

        // Act
        IReceiveMessage? result =
            dispatcher.GetMessageAsType(
                message,
                typeof(ImageDropCreateImage));

        // Assert
        result.ShouldBeNull();

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion   
}