namespace ImageDrop.AWS.SQS.Consumer.Tests.Unit.Handlers;

public sealed class ImageDropCreateImageHandlerTests
{
    private readonly IImageProcessingService _imageProcessingServiceMock;
    private readonly ILogger<ImageDropCreateImageHandler> _loggerMock;

    private readonly ImageDropCreateImageHandler _handler;

    public ImageDropCreateImageHandlerTests()
    {
        _imageProcessingServiceMock = Substitute.For<IImageProcessingService>();
        _loggerMock = Substitute.For<ILogger<ImageDropCreateImageHandler>>();

        _handler = new ImageDropCreateImageHandler(
            _imageProcessingServiceMock,
            _loggerMock);
    }

    #region HandleAsync

    [Fact]
    public async Task HandleAsync_ShouldReturnTrue_WhenImageProcessedSuccessfully()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageDropCreateImage message = new()
        {
            ImageId = imageId
        };

        _imageProcessingServiceMock
            .ProcessImage(imageId)
            .Returns(true);

        // Act
        bool result = await _handler.HandleAsync(message);

        // Assert
        result.ShouldBeTrue();

        await _imageProcessingServiceMock.Received(1)
            .ProcessImage(imageId);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenImageProcessingFailed()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageDropCreateImage message = new()
        {
            ImageId = imageId
        };

        _imageProcessingServiceMock
            .ProcessImage(imageId)
            .Returns(false);

        // Act
        bool result = await _handler.HandleAsync(message);

        // Assert
        result.ShouldBeFalse();

        await _imageProcessingServiceMock.Received(1)
            .ProcessImage(imageId);

        _loggerMock.DidNotReceive().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseAndLogError_WhenImageProcessingThrowsException()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();
        Exception exception = new InvalidOperationException("Test exception");

        ImageDropCreateImage message = new()
        {
            ImageId = imageId
        };

        _imageProcessingServiceMock
            .ProcessImage(imageId)
            .Throws(exception);

        // Act
        bool result = await _handler.HandleAsync(message);

        // Assert
        result.ShouldBeFalse();

        await _imageProcessingServiceMock.Received(1)
            .ProcessImage(imageId);

        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion
}