namespace ImageDrop.Application.Services.Abstractions;

public interface IAuthService
{
    Task<ApiResult<AuthDto>> Login(LoginDto loginDto, CancellationToken cancellationToken);
    Task<ApiResult<AuthDto>> RefreshToken(int? userId, string refreshToken, CancellationToken cancellationToken);
}