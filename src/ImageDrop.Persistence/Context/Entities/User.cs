namespace ImageDrop.Persistence.Context.Entities;

public sealed class User
{
    public int Id { get; set; }
    public required string Nickname { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserRole Role { get; set; } = UserRole.BasicUser;
    
    public ICollection<Image> Images { get; set; } = [];
}