namespace ImageDrop.Application.Dtos.User;

public sealed class UserCreateDto
{
    public required string Nickname { get; set; }
    public required string Email { get; set; }
    
    [MinLength(4)]
    public required string Password { get; set; }
    public bool StaySignedIn { get; set; }
}