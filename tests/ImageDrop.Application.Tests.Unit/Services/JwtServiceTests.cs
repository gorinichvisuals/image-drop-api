namespace ImageDrop.Application.Tests.Unit.Services;

public sealed class JwtServiceTests
{
    private readonly ICacheService _cacheServiceMock;
    private readonly IOptions<JwtOptions> _optionsMock;

    private readonly JwtService _service;

    public JwtServiceTests()
    {
        _cacheServiceMock = Substitute.For<ICacheService>();
        _optionsMock = Substitute.For<IOptions<JwtOptions>>();

        _optionsMock.Value.Returns(new JwtOptions
        {
            JwtSecretKey = "supersecrettestkey123124514123132131243141",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationDays = 1,
            RefreshTokenExpirationDays = 30,
            SigningCredentials = CreateSigningCredentials()
        });

        _service = new JwtService(
            _cacheServiceMock,
            _optionsMock);
    }

    #region GenerateTokens

    [Fact]
    public async Task GenerateTokens_ShouldReturnTokens_WhenStaySignedInIsTrue()
    {
        // Arrange
        const int userId = 1;

        User user = new()
        {
            Id = userId,
            Nickname = "Nickname",
            Email = "email@email.com",
            Password = "password",
            Role = UserRole.BasicUser
        };

        // Act
        AuthDto result = await _service.GenerateTokens(user, true);

        // Assert
        result.StaySignedIn.ShouldBeTrue();
        result.Tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        result.Tokens.RefreshToken.ShouldNotBeNullOrWhiteSpace();

        await _cacheServiceMock
            .Received(1)
            .Set(
                CacheKeyConstants.RefreshKey + userId,
                result.Tokens.RefreshToken,
                TimeSpan.FromDays(_optionsMock.Value.RefreshTokenExpirationDays));
    }

    [Fact]
    public async Task GenerateTokens_ShouldReturnOnlyAccessToken_WhenStaySignedInIsFalse()
    {
        // Arrange
        User user = new()
        {
            Id = 1,
            Nickname = "Nickname",
            Email = "email@email.com",
            Password = "password",
            Role = UserRole.BasicUser
        };

        // Act
        AuthDto result = await _service.GenerateTokens(user, false);

        // Assert
        result.StaySignedIn.ShouldBeFalse();
        result.Tokens.AccessToken.ShouldNotBeNullOrWhiteSpace();
        result.Tokens.RefreshToken.ShouldBeEmpty();

        await _cacheServiceMock
            .DidNotReceive()
            .Set(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<TimeSpan>());
    }

    #endregion

    #region Helpers
    
    private static SigningCredentials CreateSigningCredentials()
    {
        SymmetricSecurityKey key = new("supersecrettestkey123124514123132131243141"u8.ToArray());

        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }
    
    #endregion   
}