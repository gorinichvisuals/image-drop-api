namespace ImageDrop.Shared.Constants.Options;

public sealed class JwtOptions
{
    public required string JwtSecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int AccessTokenExpirationDays { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
    public int ResetPasswordTokenExpirationDays { get; set; }
    public SigningCredentials? SigningCredentials { get; set; }
}