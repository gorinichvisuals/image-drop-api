namespace ImageDrop.Api.Tests.Unit.Controllers;

public sealed class ImageControllerTests
{
    private readonly IImageService _imageServiceMock;
    private readonly ISessionProvider _sessionProviderMock;
    
    private readonly ImageController  _controller;
    
    public ImageControllerTests()
    {
        _imageServiceMock = Substitute.For<IImageService>();
        _sessionProviderMock = Substitute.For<ISessionProvider>();
        
        _controller = new ImageController(
            _imageServiceMock,
            _sessionProviderMock);
    }
    
    #region GetUserImages

    [Fact]
    public async Task GetUserImages_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        const int userId = 1;

        ApiResult<ICollection<ImageGetDto>> expectedResult = ApiResult<ICollection<ImageGetDto>>.Success(StatusCodeConstants.Ok, []);

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _imageServiceMock
            .GetUserImages(userId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.GetUserImages(
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .GetUserImages(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetUserImages_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        const int userId = 1;

        ApiResult<ICollection<ImageGetDto>> expectedResult = ApiResult<ICollection<ImageGetDto>>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _imageServiceMock
            .GetUserImages(userId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.GetUserImages(
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .GetUserImages(userId, Arg.Any<CancellationToken>());
    }

    #endregion
    
    #region GetImageById

    [Fact]
    public async Task GetImageById_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ApiResult<ImageGetDto> expectedResult = ApiResult<ImageGetDto>.Success(
            StatusCodeConstants.Ok, new ImageGetDto());

        _imageServiceMock
            .GetImageById(imageId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.GetImageById(
            imageId,
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .GetImageById(imageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetImageById_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        Guid imageId = Guid.NewGuid();

        ApiResult<ImageGetDto> expectedResult = ApiResult<ImageGetDto>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _imageServiceMock
            .GetImageById(imageId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.GetImageById(
            imageId,
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .GetImageById(imageId, Arg.Any<CancellationToken>());
    }

    #endregion
    
    #region CreateImage

    [Fact]
    public async Task CreateImage_ShouldReturnCreated_WhenServiceSucceeds()
    {
        // Arrange
        const int userId = 1;

        await using MemoryStream stream = new([1, 2, 3]);
        IFormFile file = CreateImageFile();

        ImageGetDto image = new();

        ApiResult<ImageGetDto> expectedResult = ApiResult<ImageGetDto>.Success(
            StatusCodeConstants.Created,
            image);

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _imageServiceMock
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<ImageFormat>(),
                userId)
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.CreateImage(file);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.Created);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<ImageFormat>(),
                userId);
    }

    [Fact]
    public async Task CreateImage_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        const int userId = 1;

        await using MemoryStream stream = new([1, 2, 3]);
        IFormFile file = CreateImageFile();

        ApiResult<ImageGetDto> expectedResult = ApiResult<ImageGetDto>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _imageServiceMock
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<ImageFormat>(),
                userId)
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.CreateImage(file);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .UploadImage(
                Arg.Any<Stream>(),
                Arg.Any<ImageFormat>(),
                userId);
    }

    #endregion
    
    #region DeleteImage

    [Fact]
    public async Task DeleteImage_ShouldReturnNoContent_WhenServiceSucceeds()
    {
        // Arrange
        const int userId = 1;
        Guid imageId = Guid.NewGuid();

        ApiResult expectedResult = ApiResult.Success(
            StatusCodeConstants.NoContent);

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _imageServiceMock
            .DeleteImage(imageId, userId)
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.DeleteImage(imageId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();

        await _imageServiceMock
            .Received(1)
            .DeleteImage(imageId, userId);
    }

    [Fact]
    public async Task DeleteImage_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        const int userId = 1;
        Guid imageId = Guid.NewGuid();

        ApiResult expectedResult = ApiResult.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _imageServiceMock
            .DeleteImage(imageId, userId)
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.DeleteImage(imageId);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _imageServiceMock
            .Received(1)
            .DeleteImage(imageId, userId);
    }

    #endregion
    
    #region Helpers
    
    private static IFormFile CreateImageFile()
    {
        MemoryStream stream = new([1, 2, 3]);

        return new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            "image.jpg")
        {
            Headers = new HeaderDictionary
            {
                ["Content-Type"] = "image/jpeg"
            }
        };
    }
    
    #endregion
}