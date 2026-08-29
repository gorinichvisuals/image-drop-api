namespace ImageDrop.Api.Tests.Unit.Attributes;

public sealed class ValidateImageFileAttributeTests
{
    private readonly ValidateImageFileAttribute _attribute;

    public ValidateImageFileAttributeTests()
    {
        _attribute = new ValidateImageFileAttribute();
    }

    #region OnActionExecutionAsync

    [Fact]
    public async Task OnActionExecutionAsync_ShouldReturnBadRequest_WhenFileIsMissing()
    {
        // Arrange
        ActionContext actionContext = new(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        ActionExecutingContext context = new(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);

        bool nextCalled = false;

        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;

            return Task.FromResult(
                new ActionExecutedContext(
                    context,
                    [],
                    controller: null!));
        };

        // Act
        await _attribute.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.ShouldBeFalse();

        BadRequestObjectResult result =
            context.Result.ShouldBeOfType<BadRequestObjectResult>();

        result.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

        ApiResult apiResult =
            result.Value.ShouldBeOfType<ApiResult>();

        apiResult.IsSucceed.ShouldBeFalse();
        apiResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        apiResult.ErrorCode.ShouldBe(ErrorStatusCode.INVALID_FILE.ToString());
        apiResult.ErrorMessage.ShouldBe("File is required.");
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldReturnBadRequest_WhenFileTypeIsNotSupported()
    {
        // Arrange
        IFormFile file = CreateFile("application/pdf");

        ActionExecutingContext context = CreateContext(file);

        bool nextCalled = false;

        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;

            return Task.FromResult(
                new ActionExecutedContext(
                    context,
                    [],
                    controller: null!));
        };

        // Act
        await _attribute.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.ShouldBeFalse();

        BadRequestObjectResult result =
            context.Result.ShouldBeOfType<BadRequestObjectResult>();

        result.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

        ApiResult apiResult =
            result.Value.ShouldBeOfType<ApiResult>();

        apiResult.IsSucceed.ShouldBeFalse();
        apiResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        apiResult.ErrorCode.ShouldBe(ErrorStatusCode.INVALID_FILE_TYPE.ToString());
        apiResult.ErrorMessage.ShouldBe("File type is not supported.");
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldCallNext_WhenFileTypeIsSupported()
    {
        // Arrange
        IFormFile file = CreateFile("image/jpeg");

        ActionExecutingContext context = CreateContext(file);

        bool nextCalled = false;

        ActionExecutionDelegate next = () =>
        {
            nextCalled = true;

            return Task.FromResult(
                new ActionExecutedContext(
                    context,
                    [],
                    controller: null!));
        };

        // Act
        await _attribute.OnActionExecutionAsync(context, next);

        // Assert
        nextCalled.ShouldBeTrue();
        context.Result.ShouldBeNull();
    }

    #endregion
    
    #region Helpers  

    private static ActionExecutingContext CreateContext(IFormFile file)
    {
        ActionContext actionContext = new(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        Dictionary<string, object?> actionArguments = new()
        {
            ["file"] = file
        };

        return new ActionExecutingContext(
            actionContext,
            [],
            actionArguments,
            controller: null!);
    }
    
    private static IFormFile CreateFile(string contentType)
    {
        MemoryStream stream = new([1, 2, 3]);

        return new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            "test.jpg")
        {
            Headers = new HeaderDictionary
            {
                ["Content-Type"] = contentType
            }
        };
    }

    #endregion
}