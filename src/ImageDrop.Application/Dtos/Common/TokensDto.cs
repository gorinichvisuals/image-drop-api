namespace ImageDrop.Application.Dtos.Common;

public sealed class TokensDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}