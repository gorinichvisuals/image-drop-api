namespace ImageDrop.AWS.S3.Tests.Unit.Services;

public sealed class StorageServiceTests
{
    private readonly IAmazonS3 _s3ClientMock;
    private readonly IOptions<S3Options> _optionsMock;
    private readonly ILogger<StorageService> _loggerMock;
    
    private readonly StorageService _service;

    public StorageServiceTests()
    {
        _s3ClientMock = Substitute.For<IAmazonS3>();
        _optionsMock = Substitute.For<IOptions<S3Options>>();
        _loggerMock = Substitute.For<ILogger<StorageService>>();
        
        _s3ClientMock.Config.Returns(new AmazonS3Config());
        
        _optionsMock.Value.Returns(new S3Options
        {
            PublicBaseUrl = "https://cdn.test.com",
            BucketName = "test-bucket"
        });
        
        _service = new StorageService(
            _s3ClientMock,
            _optionsMock,
            _loggerMock);
    }
    
    #region UploadImage
    
    [Fact]
    public async Task UploadImage_ShouldReturnSuccess_WhenUploadCompleted()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();
        await using MemoryStream stream = new([1, 2, 3]);

        _s3ClientMock
            .PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutObjectResponse());

        // Act
        ApiResult result = await _service.UploadImage(stream, imageId);

        // Assert
        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);

        await _s3ClientMock.Received(1).PutObjectAsync(
            Arg.Any<PutObjectRequest>(),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task UploadImage_ShouldReturnInternalServerError_WhenAmazonS3ExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();
        await using MemoryStream stream = new();

        AmazonS3Exception exception = new("S3 error");

        _s3ClientMock
            .PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult result = await _service.UploadImage(stream, imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);

        await _s3ClientMock.Received(1).PutObjectAsync(
            Arg.Any<PutObjectRequest>(),
            Arg.Any<CancellationToken>());
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task UploadImage_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();
        await using MemoryStream stream = new();

        Exception exception = new("Unexpected error");

        _s3ClientMock
            .PutObjectAsync(Arg.Any<PutObjectRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<PutObjectResponse>>(_ => throw exception);

        // Act
        ApiResult result = await _service.UploadImage(stream, imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion
    
    #region DownloadImage
    
    [Fact]
    public async Task DownloadImage_ShouldReturnStream_WhenObjectDownloadedSuccessfully()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();
        MemoryStream responseStream = new([1, 2, 3]);

        GetObjectResponse response = new()
        {
            ResponseStream = responseStream
        };

        _s3ClientMock
            .GetObjectAsync(
                _optionsMock.Value.BucketName,
                imageId.ToString(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        ApiResult<Stream> result = await _service.DownloadImage(imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        result.Data.ShouldBeSameAs(responseStream);

        await _s3ClientMock.Received(1).GetObjectAsync(
            _optionsMock.Value.BucketName,
            imageId.ToString(),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task DownloadImage_ShouldReturnInternalServerError_WhenAmazonS3ExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        AmazonS3Exception exception = new("S3 error");

        _s3ClientMock
            .GetObjectAsync(
                _optionsMock.Value.BucketName,
                imageId.ToString(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult<Stream> result = await _service.DownloadImage(imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task DownloadImage_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        Exception exception = new("Unexpected error");

        _s3ClientMock
            .GetObjectAsync(
                _optionsMock.Value.BucketName,
                imageId.ToString(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult<Stream> result = await _service.DownloadImage(imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion
    
    #region DeleteImage
    
    [Fact]
    public async Task DeleteImage_ShouldReturnSuccess_WhenObjectDeletedSuccessfully()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        _s3ClientMock
            .DeleteObjectAsync(
                _optionsMock.Value.BucketName,
                imageId.ToString(),
                Arg.Any<CancellationToken>())
            .Returns(new DeleteObjectResponse());

        // Act
        ApiResult result = await _service.DeleteImage(imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);

        await _s3ClientMock.Received(1).DeleteObjectAsync(
            _optionsMock.Value.BucketName,
            imageId.ToString(),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task DeleteImage_ShouldReturnInternalServerError_WhenAmazonS3ExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        AmazonS3Exception exception = new("S3 error");

        _s3ClientMock
            .DeleteObjectAsync(
                _optionsMock.Value.BucketName,
                imageId.ToString(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult result = await _service.DeleteImage(imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task DeleteImage_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        Exception exception = new("Unexpected error");

        _s3ClientMock
            .DeleteObjectAsync(
                _optionsMock.Value.BucketName,
                imageId.ToString(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult result = await _service.DeleteImage(imageId);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion
    
    #region DeleteImages
    
    [Fact]
    public async Task DeleteImages_ShouldReturnSuccess_AndNotCallS3_WhenImageIdsAreEmpty()
    {
        // Arrange
        List<Guid> imageIds = [];

        // Act
        ApiResult result = await _service.DeleteImages(imageIds);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);

        await _s3ClientMock
            .DidNotReceive()
            .DeleteObjectsAsync(Arg.Any<DeleteObjectsRequest>());
    }
    
    [Fact]
    public async Task DeleteImages_ShouldDeleteAllImages_WhenImageIdsAreProvided()
    {
        // Arrange
        Guid firstId = Guid.NewGuid();
        Guid secondId = Guid.NewGuid();
        Guid thirdId = Guid.NewGuid();

        List<Guid> imageIds =
        [
            firstId,
            secondId,
            thirdId
        ];

        _s3ClientMock
            .DeleteObjectsAsync(Arg.Any<DeleteObjectsRequest>(), Arg.Any<CancellationToken>())
            .Returns(new DeleteObjectsResponse());

        // Act
        ApiResult result = await _service.DeleteImages(imageIds);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);

        await _s3ClientMock.Received(1).DeleteObjectsAsync(
            Arg.Any<DeleteObjectsRequest>(),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task DeleteImages_ShouldReturnInternalServerError_WhenAmazonS3ExceptionOccurs()
    {
        // Arrange
        List<Guid> imageIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        AmazonS3Exception exception = new("S3 error");

        _s3ClientMock
            .DeleteObjectsAsync(
                Arg.Any<DeleteObjectsRequest>(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult result = await _service.DeleteImages(imageIds);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public async Task DeleteImages_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        List<Guid> imageIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        Exception exception = new("Unexpected error");

        _s3ClientMock
            .DeleteObjectsAsync(
                Arg.Any<DeleteObjectsRequest>(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult result = await _service.DeleteImages(imageIds);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
        
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    #endregion
}