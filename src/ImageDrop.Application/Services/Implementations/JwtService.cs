namespace ImageDrop.Application.Services.Implementations;

internal sealed class JwtService(ICacheService cacheService, IOptions<JwtOptions> options) : IJwtService
{    
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    
    public async Task<AuthDto> GenerateTokens(User user, bool staySignedIn)
    {
        AuthDto result = new()
        {
            Tokens = new TokensDto()
            {
                AccessToken =  CreateAccessToken(user.Id, user.Role),
                RefreshToken = staySignedIn
                    ? CreateRefreshToken(user.Id)
                    : string.Empty,
            },
            StaySignedIn = staySignedIn,
        };

        if (staySignedIn)
        {
            string refreshTokenKey = CacheKeyConstants.RefreshKey + user.Id;

            await cacheService.Set(refreshTokenKey, result.Tokens.RefreshToken, TimeSpan.FromDays(options.Value.RefreshTokenExpirationDays));
        }

        return result;
    }
    
    private string CreateAccessToken(int userId, UserRole role)
    {
        Claim[] claims =
        [
            new(UserClaimConstants.Id, userId.ToString()!),
            new(UserClaimConstants.Role, role.ToString()),
        ];

        JwtSecurityToken token = new(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(Convert.ToDouble(options.Value.AccessTokenExpirationDays)),
            signingCredentials: options.Value.SigningCredentials);

        return _tokenHandler.WriteToken(token);
    }

    private string CreateRefreshToken(int userId)
    {
        Claim[] claims =
        [            
            new(UserClaimConstants.Id, userId.ToString()),
            new(UserClaimConstants.Role, DefaultAuthScopeConstants.RefreshTokenRole),
        ];

        JwtSecurityToken jwt = new(
            options.Value.Issuer,
            options.Value.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(options.Value.RefreshTokenExpirationDays),
            options.Value.SigningCredentials);

        return _tokenHandler.WriteToken(jwt);
    }
}