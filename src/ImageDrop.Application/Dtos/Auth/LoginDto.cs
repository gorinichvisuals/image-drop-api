namespace ImageDrop.Application.Dtos.Auth;

public class LoginDto
{
    [EmailAddress]
    public required string Login { get; set; }

    [MinLength(4)]
    public required string Password { get; set; }
    
    public bool StaySignedIn { get; set; }
}