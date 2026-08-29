using Isopoh.Cryptography.Argon2;

namespace ImageDrop.Application.Tests.Unit.Services;

public sealed class AuthServiceTests
{
    private readonly IJwtService _jwtServiceMock; 
    private readonly ICacheService _cacheServiceMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _jwtServiceMock = Substitute.For<IJwtService>();
        _cacheServiceMock = Substitute.For<ICacheService>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        
        _service = new AuthService(
            _jwtServiceMock,
            _cacheServiceMock,
            _unitOfWorkMock);
    }

    #region Login
    
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        LoginDto loginDto = new()
        {
            Login = "test@test.com",
            Password = "Password123",
            StaySignedIn = true
        };

        _unitOfWorkMock.UserRepository
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        ApiResult<AuthDto> result = await _service.Login(
            loginDto,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Unauthorized);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.USER_INVALID_CREDENTIALS));
        result.ErrorMessage.ShouldBe("User invalid email.");

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        LoginDto loginDto = new()
        {
            Login = "test@test.com",
            Password = "WrongPassword",
            StaySignedIn = true
        };

        User user = new()
        {
            Id = 1,
            Nickname = "Nickname",
            Email = loginDto.Login,
            Password = Argon2.Hash("CorrectPassword")
        };

        _unitOfWorkMock.UserRepository
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        ApiResult<AuthDto> result = await _service.Login(
            loginDto,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Unauthorized);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.USER_PASSWORD_IS_NOT_VALID));
        result.ErrorMessage.ShouldBe("Incorrect password.");

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Login_ShouldReturnSuccess_WhenCredentialsAreCorrect()
    {
        // Arrange
        LoginDto loginDto = new()
        {
            Login = "test@test.com",
            Password = "CorrectPassword",
            StaySignedIn = true
        };

        User user = new()
        {
            Id = 1,
            Nickname = "Nickname",
            Email = loginDto.Login,
            Password = Argon2.Hash(loginDto.Password)
        };

        AuthDto expectedResult = new();

        _unitOfWorkMock.UserRepository
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        _jwtServiceMock
            .GenerateTokens(user, loginDto.StaySignedIn)
            .Returns(expectedResult);

        // Act
        ApiResult<AuthDto> result = await _service.Login(
            loginDto,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        result.Data.ShouldBe(expectedResult);

        await _jwtServiceMock
            .Received(1)
            .GenerateTokens(user, loginDto.StaySignedIn);
    }

    [Fact]
    public async Task Login_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        LoginDto loginDto = new()
        {
            Login = "test@test.com",
            Password = "Password123",
            StaySignedIn = true
        };

        Exception exception = new("Something went wrong.");

        _unitOfWorkMock.UserRepository
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        ApiResult<AuthDto> result = await _service.Login(
            loginDto,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
    }

    #endregion
    
    #region RefreshToken
    
    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenDoesNotExist()
    {
        // Arrange
        int userId = 1;
        string refreshToken = "refresh-token";

        _cacheServiceMock
            .TryGet<string>(CacheKeyConstants.RefreshKey + userId)
            .Returns((false, null));

        // Act
        ApiResult<AuthDto> result = await _service.RefreshToken(
            userId,
            refreshToken,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Unauthorized);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.USER_INVALID_REFRESH_TOKEN));
        result.ErrorMessage.ShouldBe("Invalid or expired refresh token.");

        await _unitOfWorkMock.UserRepository
            .DidNotReceive()
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>());

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        int userId = 1;
        string refreshToken = "invalid-token";
        string existingRefreshToken = "valid-token";

        _cacheServiceMock
            .TryGet<string>(CacheKeyConstants.RefreshKey + userId)
            .Returns((true, existingRefreshToken));

        // Act
        ApiResult<AuthDto> result = await _service.RefreshToken(
            userId,
            refreshToken,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Unauthorized);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.USER_INVALID_REFRESH_TOKEN));
        result.ErrorMessage.ShouldBe("Invalid or expired refresh token.");

        await _unitOfWorkMock.UserRepository
            .DidNotReceive()
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>());

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        int userId = 1;
        string refreshToken = "valid-token";

        _cacheServiceMock
            .TryGet<string>(CacheKeyConstants.RefreshKey + userId)
            .Returns((true, refreshToken));

        _unitOfWorkMock.UserRepository
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        ApiResult<AuthDto> result = await _service.RefreshToken(
            userId,
            refreshToken,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Unauthorized);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.USER_NOT_FOUND));
        result.ErrorMessage.ShouldBe("User not found.");

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(Arg.Any<User>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnSuccess_WhenRefreshTokenAndUserAreValid()
    {
        // Arrange
        int userId = 1;
        string refreshToken = "valid-token";

        User user = new()
        {
            Id = userId,
            Nickname = "nickname",
            Email = "test@test.com",
            Password = Argon2.Hash("Password123")
        };

        AuthDto expectedResult = new();

        _cacheServiceMock
            .TryGet<string>(CacheKeyConstants.RefreshKey + userId)
            .Returns((true, refreshToken));

        _unitOfWorkMock.UserRepository
            .GetEntityWithoutTracking(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(user);

        _jwtServiceMock
            .GenerateTokens(user, true)
            .Returns(expectedResult);

        // Act
        ApiResult<AuthDto> result = await _service.RefreshToken(
            userId,
            refreshToken,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        result.Data.ShouldBe(expectedResult);

        await _jwtServiceMock
            .Received(1)
            .GenerateTokens(user, true);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        int userId = 1;
        string refreshToken = "refresh-token";

        Exception exception = new("Something went wrong.");

        _cacheServiceMock
            .TryGet<string>(CacheKeyConstants.RefreshKey + userId)
            .Throws(exception);

        // Act
        ApiResult<AuthDto> result = await _service.RefreshToken(
            userId,
            refreshToken,
            CancellationToken.None);

        // Assert
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(nameof(ErrorStatusCode.INTERNAL_SERVER_ERROR));
        result.ErrorMessage.ShouldBe(exception.Message);
    }
    
    #endregion
}