namespace ImageDrop.AWS.SQS.Consumer.Tests.Unit.Helpers;

public sealed class ImageDropHelperTests
{
    private readonly ImageDropHelper _helper = new();

    #region GetMessageMappings
    
    [Fact]
    public void GetMessageMappings_ShouldReturnMessageTypes()
    {
        // Act
        IReadOnlyDictionary<string, Type> result =
            _helper.GetMessageMappings();

        // Assert
        result.ShouldNotBeEmpty();

        result.ShouldContainKey(nameof(ImageDropCreateImage));
        result[nameof(ImageDropCreateImage)]
            .ShouldBe(typeof(ImageDropCreateImage));
    }

    [Fact]
    public void GetMessageMappings_ShouldReturnOnlyConcreteMessageTypes()
    {
        // Act
        IReadOnlyDictionary<string, Type> result =
            _helper.GetMessageMappings();

        // Assert
        result.Values.ShouldAllBe(type =>
            typeof(IMessage).IsAssignableFrom(type) &&
            !type.IsInterface &&
            !type.IsAbstract);
    }
    
    #endregion
    
    #region GetHandlers

    [Fact]
    public void GetHandlers_ShouldReturnHandlerMappings()
    {
        // Act
        IReadOnlyDictionary<string, Func<IServiceProvider, IMessageHandler>> result =
            _helper.GetHandlers();

        // Assert
        result.ShouldNotBeEmpty();

        result.ShouldContainKey(nameof(ImageDropCreateImage));
    }

    [Fact]
    public void GetHandlers_ShouldResolveCorrectHandlerFromServiceProvider()
    {
        // Arrange
        IReadOnlyDictionary<string, Func<IServiceProvider, IMessageHandler>> handlers =
            _helper.GetHandlers();

        ServiceCollection services = new();

        IImageProcessingService imageProcessingServiceMock =
            Substitute.For<IImageProcessingService>();

        ILogger<ImageDropCreateImageHandler> loggerMock =
            Substitute.For<ILogger<ImageDropCreateImageHandler>>();

        services.AddSingleton(imageProcessingServiceMock);
        services.AddSingleton(loggerMock);
        services.AddTransient<ImageDropCreateImageHandler>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act
        IMessageHandler result =
            handlers[nameof(ImageDropCreateImage)](serviceProvider);

        // Assert
        result.ShouldBeOfType<ImageDropCreateImageHandler>();
    }
    
    #endregion
}