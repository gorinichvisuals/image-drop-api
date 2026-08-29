namespace ImageDrop.Shared.Services.Services.Implementations;

internal sealed class ImageCompressorService(IOptions<ImageProcessingOptions> options) : IImageCompressorService
{
    private readonly ImageProcessingOptions _options = options.Value;
    
    public async Task<Stream> Compress(Stream sourceStream, ImageFormat format)
    {
        using Image image = await Image.LoadAsync(sourceStream);
        MemoryStream output = new();

        switch (format)
        {
            case ImageFormat.Jpeg:
                await image.SaveAsJpegAsync(output, new JpegEncoder() { Quality = _options.JpegQuality });
                break;
            
            case ImageFormat.Png:
                await image.SaveAsPngAsync(output,
                    new PngEncoder() { CompressionLevel = (PngCompressionLevel)_options.PngCompressionLevel });
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        output.Position = 0;
        
        return output;
    }
}