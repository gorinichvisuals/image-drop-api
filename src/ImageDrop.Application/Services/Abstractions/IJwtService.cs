namespace ImageDrop.Application.Services.Abstractions;

public interface IJwtService
{
    Task<AuthDto> GenerateTokens(User user, bool staySignedIn);
}