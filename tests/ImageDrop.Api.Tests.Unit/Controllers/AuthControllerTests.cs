namespace ImageDrop.Api.Tests.Unit.Controllers;

public sealed class AuthControllerTests
{
    private readonly IAuthService _authServiceMock;
    private readonly ISessionProvider _sessionProviderMock;
    
    private readonly AuthController  _controller;
    
    public AuthControllerTests()
    {
        _authServiceMock = Substitute.For<IAuthService>();
        _sessionProviderMock = Substitute.For<ISessionProvider>();
        
        _controller = new AuthController(
            _authServiceMock, 
            _sessionProviderMock);
    }
    
    #region Login

    [Fact]
    public async Task Login_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        LoginDto loginDto = new() { Login = "test", Password = "test" };

        AuthDto authDto = new();

        ApiResult<AuthDto> expectedResult = ApiResult<AuthDto>.Success(
            StatusCodeConstants.Ok,
            authDto);

        _authServiceMock
            .Login(loginDto, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.Login(
            loginDto,
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _authServiceMock
            .Received(1)
            .Login(loginDto, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        LoginDto loginDto = new() { Login = "test", Password = "test" };

        ApiResult<AuthDto> expectedResult = ApiResult<AuthDto>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _authServiceMock
            .Login(loginDto, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.Login(
            loginDto,
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _authServiceMock
            .Received(1)
            .Login(loginDto, Arg.Any<CancellationToken>());
    }

    #endregion
    
    #region RefreshToken

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        const int userId = 1;
        const string refreshToken = "refresh-token";

        AuthDto authDto = new();

        ApiResult<AuthDto> expectedResult = ApiResult<AuthDto>.Success(
            StatusCodeConstants.Ok,
            authDto);

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _sessionProviderMock
            .GetUserSessionToken()
            .Returns(refreshToken);

        _authServiceMock
            .RefreshToken(
                userId,
                refreshToken,
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.RefreshToken(
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _authServiceMock
            .Received(1)
            .RefreshToken(
                userId,
                refreshToken,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        const int userId = 1;
        const string refreshToken = "refresh-token";

        ApiResult<AuthDto> expectedResult = ApiResult<AuthDto>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _sessionProviderMock
            .GetUserId()
            .Returns(userId);

        _sessionProviderMock
            .GetUserSessionToken()
            .Returns(refreshToken);

        _authServiceMock
            .RefreshToken(
                userId,
                refreshToken,
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.RefreshToken(
            CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _authServiceMock
            .Received(1)
            .RefreshToken(
                userId,
                refreshToken,
                Arg.Any<CancellationToken>());
    }

    #endregion
}