namespace ImageDrop.Application.Services.Implementations;

internal sealed class AuthService(
    IJwtService jwtService,    
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IAuthService
{
    private const bool StaySignedIn = true;

    public async Task<ApiResult<AuthDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await unitOfWork.UserRepository
                .GetEntityWithoutTracking(user => user.Email == loginDto.Login, cancellationToken);

            if (user is null)
                return ApiResult<AuthDto>.Fail(StatusCodeConstants.Unauthorized, ErrorStatusCode.USER_INVALID_CREDENTIALS, "User invalid email.");

            bool isPasswordCorrect = Argon2.Verify(user.Password, loginDto.Password);

            if (!isPasswordCorrect)
                return ApiResult<AuthDto>.Fail(StatusCodeConstants.Unauthorized, ErrorStatusCode.USER_PASSWORD_IS_NOT_VALID, "Incorrect password.");
            
            AuthDto authenticationResult = await jwtService.GenerateTokens(user, loginDto.StaySignedIn);
                
            return ApiResult<AuthDto>.Success(StatusCodeConstants.Ok, authenticationResult);  
        }
        catch (Exception exception)
        {
            return ApiResult<AuthDto>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }

    public async Task<ApiResult<AuthDto>> RefreshToken(int? userId, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            string refreshTokenKey = CacheKeyConstants.RefreshKey + userId;

            (bool tokenExists, string? existingRefreshToken) = await cacheService.TryGet<string>(refreshTokenKey);

            if (!tokenExists || existingRefreshToken != refreshToken)
                return ApiResult<AuthDto>.Fail(StatusCodeConstants.Unauthorized, ErrorStatusCode.USER_INVALID_REFRESH_TOKEN, "Invalid or expired refresh token.");

            User? user = await unitOfWork.UserRepository.GetEntityWithoutTracking(customer => customer.Id == userId, cancellationToken);

            if (user is null)
                return ApiResult<AuthDto>.Fail(StatusCodeConstants.Unauthorized, ErrorStatusCode.USER_NOT_FOUND, "User not found.");
            
            AuthDto authenticationResult = await jwtService.GenerateTokens(user, StaySignedIn);
            
            return ApiResult<AuthDto>.Success(StatusCodeConstants.Ok, authenticationResult);
        }
        catch (Exception exception)
        {
            return ApiResult<AuthDto>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }
}