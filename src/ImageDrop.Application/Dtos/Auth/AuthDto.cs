namespace ImageDrop.Application.Dtos.Auth;

public sealed class AuthDto
{
    public TokensDto Tokens { get; set; } = null!;
    public bool StaySignedIn { get; set; }
}