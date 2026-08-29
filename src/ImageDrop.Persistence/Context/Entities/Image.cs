namespace ImageDrop.Persistence.Context.Entities;

public sealed class Image
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ImageProcessingStatus ProcessingStatus { get; set; } = ImageProcessingStatus.Processing;
    public ImageFormat Format { get; set; } = ImageFormat.Jpeg; 
    
    public User? User { get; set; }
}