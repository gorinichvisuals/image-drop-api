namespace ImageDrop.Application.Tests.Unit.Services;

public sealed class ImageServiceTests
{
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IStorageService _storageServiceMock;
    private readonly ISqsPublisher _sqsPublisherMock;
    private readonly IOptions<S3Options> _optionsMock;
    
    private readonly ImageService _service;

    public ImageServiceTests()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _storageServiceMock = Substitute.For<IStorageService>();
        _sqsPublisherMock = Substitute.For<ISqsPublisher>();
        _optionsMock = Substitute.For<IOptions<S3Options>>();

        _optionsMock.Value.Returns(new S3Options
        {
            PublicBaseUrl = "https://cdn.test.com",
            BucketName = "test-bucket"
        });

        _service = new ImageService(
            _unitOfWorkMock,
            _storageServiceMock,
            _sqsPublisherMock,
            _optionsMock);
    }
    
    #region GetUserImages

    [Fact]
    public async Task GetUserImages_ShouldReturnImages_WhenServiceSucceeds()
    {
        // Arrange
        const int userId = 1;
        Guid imageId1 = Guid.CreateVersion7();
        Guid imageId2 = Guid.CreateVersion7();

        ICollection<ImageGetDto> images =
        [
            new() { ImageId = imageId1 },
            new() { ImageId = imageId2 }
        ];

        _unitOfWorkMock.ImageRepository
            .GetSelectedItems(
                Arg.Any<Expression<Func<Image, ImageGetDto>>>(),
                Arg.Any<Expression<Func<Image, bool>>>(),
                Arg.Any<Expression<Func<Image, object>>>(),
                Arg.Any<CancellationToken>())
            .Returns(images);

        // Act
        ApiResult<ICollection<ImageGetDto>> result =
            await _service.GetUserImages(
                userId,
                CancellationToken.None);

        // Assert
        result.IsSucceed.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        result.Data.ShouldBeSameAs(images);

        images.ShouldAllBe(image => image.Url == $"https://cdn.test.com/{image.ImageId}");
    }

    [Fact]
    public async Task GetUserImages_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        const int userId = 1;

        _unitOfWorkMock.ImageRepository
            .GetSelectedItems(
                Arg.Any<Expression<Func<Image, ImageGetDto>>>(),
                Arg.Any<Expression<Func<Image, bool>>>(),
                Arg.Any<Expression<Func<Image, object>>>(),
                Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        // Act
        ApiResult<ICollection<ImageGetDto>> result =
            await _service.GetUserImages(
                userId,
                CancellationToken.None);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe("Database error");
    }

    #endregion
    
    #region GetImageById

    [Fact]
    public async Task GetImageById_ShouldReturnImage_WhenImageExists()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ImageGetDto image = new()
        {
            ImageId = imageId
        };

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<Image, ImageGetDto>>>(),
                Arg.Any<Expression<Func<Image, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(image);

        // Act
        ApiResult<ImageGetDto> result =
            await _service.GetImageById(
                imageId,
                CancellationToken.None);

        // Assert
        result.IsSucceed.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        result.Data.ShouldBeSameAs(image);
        result.Data!.Url.ShouldBe($"https://cdn.test.com/{imageId}");
    }

    [Fact]
    public async Task GetImageById_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<Image, ImageGetDto>>>(),
                Arg.Any<Expression<Func<Image, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((ImageGetDto?)null);

        // Act
        ApiResult<ImageGetDto> result =
            await _service.GetImageById(
                imageId,
                CancellationToken.None);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.NotFound);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.IMAGE_NOT_FOUND));
        result.ErrorMessage.ShouldBe("Image not found.");
    }
    
    [Fact]
    public async Task GetImageById_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        _unitOfWorkMock.ImageRepository
            .GetSelectedItem(
                Arg.Any<Expression<Func<Image, ImageGetDto>>>(),
                Arg.Any<Expression<Func<Image, bool>>>(),
                Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        // Act
        ApiResult<ImageGetDto> result =
            await _service.GetImageById(
                imageId,
                CancellationToken.None);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe("Database error");
    }

    #endregion
    
    #region UploadImage

    [Fact]
    public async Task UploadImage_ShouldCreateImageAndReturnCreated_WhenUploadSucceeds()
    {
        // Arrange
        const int userId = 1;

        await using MemoryStream stream = new([1, 2, 3]);

        ImageFormat format = ImageFormat.Jpeg;

        _storageServiceMock
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<Guid>())
            .Returns(ApiResult.Success(StatusCodeConstants.Created));

        _sqsPublisherMock
            .PublishToImageDropQueue(
                Arg.Any<ImageDropCreateImage>(),
                MessageGroupIdConstants.ImageDropGroupId)
            .Returns(Task.CompletedTask);

        // Act
        ApiResult<ImageGetDto> result =
            await _service.UploadImage(
                stream,
                format,
                userId);

        // Assert
        result.IsSucceed.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodeConstants.Created);
        result.Data.ShouldNotBeNull();

        result.Data.ImageId.ShouldNotBe(Guid.Empty);
        result.Data.Url.ShouldBe(
            $"https://cdn.test.com/{result.Data.ImageId}");

        await _unitOfWorkMock.ImageRepository
            .Received(1)
            .Add(Arg.Is<Image>(image =>
                image.UserId == userId &&
                image.Format == format));

        await _unitOfWorkMock
            .Received(1)
            .Save();

        await _storageServiceMock
            .Received(1)
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Is<Guid>(id => id != Guid.Empty));

        await _sqsPublisherMock
            .Received(1)
            .PublishToImageDropQueue(
                Arg.Is<ImageDropCreateImage>(message =>
                    message.ImageId != Guid.Empty),
                MessageGroupIdConstants.ImageDropGroupId);
    }

    [Fact]
    public async Task UploadImage_ShouldReturnError_WhenStorageUploadFails()
    {
        // Arrange
        await using MemoryStream stream = new([1, 2, 3]);

        ApiResult storageResult = ApiResult.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Upload failed");

        _storageServiceMock
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<Guid>())
            .Returns(storageResult);

        // Act
        ApiResult<ImageGetDto> result =
            await _service.UploadImage(
                stream,
                ImageFormat.Jpeg);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe("Upload failed");

        await _sqsPublisherMock
            .DidNotReceive()
            .PublishToImageDropQueue(
                Arg.Any<ImageDropCreateImage>(),
                Arg.Any<string>());
    }
    
    [Fact]
    public async Task UploadImage_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        await using MemoryStream stream = new([1, 2, 3]);

        const string exceptionMessage = "Database error";

        _unitOfWorkMock
            .Save()
            .Returns<Task>(_ => throw new Exception(exceptionMessage));

        // Act
        ApiResult<ImageGetDto> result = await _service.UploadImage(
            stream,
            ImageFormat.Jpeg);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exceptionMessage);

        await _storageServiceMock
            .DidNotReceive()
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<Guid>());

        await _sqsPublisherMock
            .DidNotReceive()
            .PublishToImageDropQueue(
                Arg.Any<ImageDropCreateImage>(),
                Arg.Any<string>());
    }

    #endregion
    
    #region DeleteImage

    [Fact]
    public async Task DeleteImage_ShouldReturnNoContent_WhenImageIsDeleted()
    {
        // Arrange
        const int userId = 1;
        Guid imageId = Guid.NewGuid();

        _unitOfWorkMock.ImageRepository
            .Any(Arg.Any<Expression<Func<Image, bool>>>())
            .Returns(true);

        _storageServiceMock
            .DeleteImage(imageId)
            .Returns(ApiResult.Success(StatusCodeConstants.NoContent));

        // Act
        ApiResult result =
            await _service.DeleteImage(
                imageId,
                userId);

        // Assert
        result.IsSucceed.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodeConstants.NoContent);

        await _storageServiceMock
            .Received(1)
            .DeleteImage(imageId);

        await _unitOfWorkMock.ImageRepository
            .Received(1)
            .Delete(Arg.Any<Expression<Func<Image, bool>>>());
    }

    [Fact]
    public async Task DeleteImage_ShouldReturnNotFound_WhenImageDoesNotExist()
    {
        // Arrange
        const int userId = 1;
        Guid imageId = Guid.NewGuid();

        _unitOfWorkMock.ImageRepository
            .Any(Arg.Any<Expression<Func<Image, bool>>>())
            .Returns(false);

        // Act
        ApiResult result =
            await _service.DeleteImage(
                imageId,
                userId);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.NotFound);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.IMAGE_NOT_FOUND));
        result.ErrorMessage.ShouldBe("Image not found, in processing or already deleted.");

        await _storageServiceMock
            .DidNotReceive()
            .DeleteImage(imageId);

        await _unitOfWorkMock.ImageRepository
            .DidNotReceive()
            .Delete(Arg.Any<Expression<Func<Image, bool>>>());
    }
    
    [Fact]
    public async Task DeleteImage_ShouldReturnInternalServerError_WhenErrorOccurs()
    {
        // Arrange
        const int userId = 1;
        Guid imageId = Guid.NewGuid();

        _unitOfWorkMock.ImageRepository
            .Any(Arg.Any<Expression<Func<Image, bool>>>())
            .Throws(new Exception("Database error."));

        // Act
        ApiResult result =
            await _service.DeleteImage(
                imageId,
                userId);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe("Database error.");

        await _storageServiceMock
            .DidNotReceive()
            .DeleteImage(imageId);

        await _unitOfWorkMock.ImageRepository
            .DidNotReceive()
            .Delete(Arg.Any<Expression<Func<Image, bool>>>());
    }

    #endregion
}