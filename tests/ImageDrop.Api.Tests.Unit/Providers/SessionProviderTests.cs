namespace ImageDrop.Api.Tests.Unit.Providers;

public sealed class SessionProviderTests
{
    private readonly IHttpContextAccessor _httpContextAccessorMock;
    private readonly SessionProvider _sessionProvider;

    public SessionProviderTests()
    {
        _httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        _sessionProvider = new SessionProvider(_httpContextAccessorMock);
    }
    
    #region GetUserSessionToken

    [Fact]
    public void GetUserSessionToken_ShouldReturnTokenWithoutBearerPrefix()
    {
        // Arrange
        DefaultHttpContext context = new();

        context.Request.Headers.Authorization = "Bearer refresh-token";

        _httpContextAccessorMock
            .HttpContext
            .Returns(context);

        // Act
        string result = _sessionProvider.GetUserSessionToken();

        // Assert
        result.ShouldBe("refresh-token");
    }

    [Fact]
    public void GetUserSessionToken_ShouldReturnAuthorizationValue_WhenBearerPrefixIsMissing()
    {
        // Arrange
        DefaultHttpContext context = new();

        context.Request.Headers.Authorization = "refresh-token";

        _httpContextAccessorMock
            .HttpContext
            .Returns(context);

        // Act
        string result = _sessionProvider.GetUserSessionToken();

        // Assert
        result.ShouldBe("refresh-token");
    }

    #endregion
    
    #region GetUserId

    [Fact]
    public void GetUserId_ShouldReturnUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        const int userId = 123;

        ClaimsPrincipal user = new(
            new ClaimsIdentity(
            [
                new Claim(
                    UserClaimConstants.Id,
                    userId.ToString())
            ],
            authenticationType: "TestAuth"));

        DefaultHttpContext context = new()
        {
            User = user
        };

        _httpContextAccessorMock
            .HttpContext
            .Returns(context);

        // Act
        int? result = _sessionProvider.GetUserId();

        // Assert
        result.ShouldBe(userId);
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenUserIsNotAuthenticated()
    {
        // Arrange
        ClaimsPrincipal user = new(
            new ClaimsIdentity());

        DefaultHttpContext context = new()
        {
            User = user
        };

        _httpContextAccessorMock
            .HttpContext
            .Returns(context);

        // Act
        int? result = _sessionProvider.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock
            .HttpContext
            .Returns((HttpContext?)null);

        // Act
        int? result = _sessionProvider.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenUserIdClaimIsMissing()
    {
        // Arrange
        ClaimsPrincipal user = new(
            new ClaimsIdentity(
                authenticationType: "TestAuth"));

        DefaultHttpContext context = new()
        {
            User = user
        };

        _httpContextAccessorMock
            .HttpContext
            .Returns(context);

        // Act
        int? result = _sessionProvider.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenUserIdClaimIsNotInteger()
    {
        // Arrange
        ClaimsPrincipal user = new(
            new ClaimsIdentity(
            [
                new Claim(
                    UserClaimConstants.Id,
                    "not-an-integer")
            ],
            authenticationType: "TestAuth"));

        DefaultHttpContext context = new()
        {
            User = user
        };

        _httpContextAccessorMock
            .HttpContext
            .Returns(context);

        // Act
        int? result = _sessionProvider.GetUserId();

        // Assert
        result.ShouldBeNull();
    }

    #endregion
}