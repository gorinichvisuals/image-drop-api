namespace ImageDrop.Shared.Services.Tests.Unit.Services;

public sealed class ImageProcessingServiceTests
{
    private readonly IImageCompressorService _imageCompressorServiceMock;
    private readonly IStorageService _storageServiceMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<ImageProcessingService> _loggerMock;
    private readonly IOptions<ImageCleanupOptions> _optionsMock;
    
    private readonly ImageProcessingService _service;

    public ImageProcessingServiceTests()
    {
        _imageCompressorServiceMock = Substitute.For<IImageCompressorService>();
        _storageServiceMock = Substitute.For<IStorageService>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<ImageProcessingService>>();
        _optionsMock = Substitute.For<IOptions<ImageCleanupOptions>>();

        _optionsMock.Value.Returns(new ImageCleanupOptions()
        {
            UnownedImageRetentionDays = 10,
            MaxDeleteObjectsBatchSize = 1000,
            IntervalInHours = 6,
            TriggerIdentity = "test",
        });
            
        _service = new ImageProcessingService(
            _imageCompressorServiceMock,
            _storageServiceMock,
            _unitOfWorkMock,
            _optionsMock,
            _loggerMock);
    }

    #region ProcessImage

    [Fact]
    public async Task ProcessImage_ShouldReturnFalseAndSetFailed_WhenImageNotFound()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, ImageGetFormatDto>>>(),
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>())
            .Returns((ImageGetFormatDto?)null);

        // Act
        bool result = await _service.ProcessImage(imageId);

        // Assert
        result.ShouldBeFalse();

        await _unitOfWorkMock.ImageRepository.Received(1)
            .UpdateImageProcessingStatus(
                imageId,
                ImageProcessingStatus.Failed);

        await _storageServiceMock.DidNotReceive()
            .DownloadImage(Arg.Any<Guid>());

        await _imageCompressorServiceMock.DidNotReceive()
            .Compress(
                Arg.Any<Stream>(),
                Arg.Any<ImageFormat>());

        await _storageServiceMock.DidNotReceive()
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<Guid>());
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task ProcessImage_ShouldReturnFalseAndSetFailed_WhenDownloadFails()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageFormat format = ImageFormat.Jpeg;

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, ImageGetFormatDto>>>(),
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>())
            .Returns(new ImageGetFormatDto
            {
                Format = format
            });

        ApiResult<Stream> downloadResult = ApiResult<Stream>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Download failed");

        _storageServiceMock
            .DownloadImage(imageId)
            .Returns(downloadResult);

        // Act
        bool result = await _service.ProcessImage(imageId);

        // Assert
        result.ShouldBeFalse();

        await _storageServiceMock.Received(1)
            .DownloadImage(imageId);

        await _unitOfWorkMock.ImageRepository.Received(1)
            .UpdateImageProcessingStatus(
                imageId,
                ImageProcessingStatus.Failed);

        await _imageCompressorServiceMock.DidNotReceive()
            .Compress(
                Arg.Any<Stream>(),
                Arg.Any<ImageFormat>());

        await _storageServiceMock.DidNotReceive()
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<Guid>());
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task ProcessImage_ShouldReturnTrueAndSetSucceed_WhenImageProcessedSuccessfully()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageFormat format = ImageFormat.Jpeg;

        MemoryStream sourceStream = new([1, 2, 3]);
        MemoryStream compressedStream = new([4, 5, 6]);

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, ImageGetFormatDto>>>(),
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>())
            .Returns(new ImageGetFormatDto
            {
                Format = format
            });

        _storageServiceMock
            .DownloadImage(imageId)
            .Returns(ApiResult<Stream>.Success(
                StatusCodeConstants.Ok,
                sourceStream));

        _imageCompressorServiceMock
            .Compress(sourceStream, format)
            .Returns(compressedStream);

        _storageServiceMock
            .UploadImage(compressedStream, imageId)
            .Returns(ApiResult.Success(StatusCodeConstants.Ok));

        // Act
        bool result = await _service.ProcessImage(imageId);

        // Assert
        result.ShouldBeTrue();

        await _storageServiceMock.Received(1)
            .DownloadImage(imageId);

        await _imageCompressorServiceMock.Received(1)
            .Compress(sourceStream, format);

        await _storageServiceMock.Received(1)
            .UploadImage(compressedStream, imageId);

        await _unitOfWorkMock.ImageRepository.Received(1)
            .UpdateImageProcessingStatus(
                imageId,
                ImageProcessingStatus.Succeed);
        
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task ProcessImage_ShouldReturnFalseAndSetFailed_WhenUploadFails()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageFormat format = ImageFormat.Png;

        MemoryStream sourceStream = new([1, 2, 3]);
        MemoryStream compressedStream = new([4, 5, 6]);

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, ImageGetFormatDto>>>(),
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>())
            .Returns(new ImageGetFormatDto
            {
                Format = format
            });

        _storageServiceMock
            .DownloadImage(imageId)
            .Returns(ApiResult<Stream>.Success(
                StatusCodeConstants.Ok,
                sourceStream));

        _imageCompressorServiceMock
            .Compress(sourceStream, format)
            .Returns(compressedStream);

        _storageServiceMock
            .UploadImage(compressedStream, imageId)
            .Returns(ApiResult.Fail(
                StatusCodeConstants.InternalServerError,
                ErrorStatusCode.INTERNAL_SERVER_ERROR,
                "Upload failed"));

        // Act
        bool result = await _service.ProcessImage(imageId);

        // Assert
        result.ShouldBeFalse();

        await _imageCompressorServiceMock.Received(1)
            .Compress(sourceStream, format);

        await _storageServiceMock.Received(1)
            .UploadImage(compressedStream, imageId);

        await _unitOfWorkMock.ImageRepository.Received(1)
            .UpdateImageProcessingStatus(
                imageId,
                ImageProcessingStatus.Failed);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion

    #region ImagesCleanup

    [Fact]
    public async Task ImagesCleanup_ShouldReturnWithoutDeleting_WhenNoImagesFound()
    {
        // Arrange
        _unitOfWorkMock.ImageRepository
            .GetSelectedItems(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, Guid>>>(),
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>())
            .Returns(new List<Guid>());

        // Act
        await _service.ImagesCleanup();

        // Assert
        await _storageServiceMock.DidNotReceive()
            .DeleteImages(Arg.Any<ICollection<Guid>>());

        await _unitOfWorkMock.ImageRepository.DidNotReceive()
            .Delete(Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>());

        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ImagesCleanup_ShouldDeleteImagesAndLog_WhenImagesFound()
    {
        // Arrange
        List<Guid> imageIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        _unitOfWorkMock.ImageRepository
            .GetSelectedItems(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, Guid>>>(),
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>())
            .Returns(imageIds);

        // Act
        await _service.ImagesCleanup();

        // Assert
        await _storageServiceMock.Received(1)
            .DeleteImages(
                Arg.Is<ICollection<Guid>>(ids =>
                    ids.Count == imageIds.Count &&
                    ids.All(imageIds.Contains)));

        await _unitOfWorkMock.ImageRepository.Received(1)
            .Delete(
                Arg.Any<Expression<Func<ImageDrop.Persistence.Context.Entities.Image, bool>>>());

        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion
}