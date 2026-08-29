namespace ImageDrop.Application.Dtos.Image;

public sealed class ImageGetDto
{
    public Guid ImageId { get; set; }
    public string Url { get; set; } = string.Empty;
}