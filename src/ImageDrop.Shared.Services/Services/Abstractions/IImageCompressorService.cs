namespace ImageDrop.Shared.Services.Services.Abstractions;

public interface IImageCompressorService
{
    Task<Stream > Compress(Stream sourceStream, ImageFormat format);
}