namespace ImageDrop.Shared.Services.Tests.Unit.Services;

public sealed class ImageCompressorServiceTests
{
    private readonly IOptions<ImageProcessingOptions> _optionsMock;
    
    private readonly ImageCompressorService _service;
    
    public ImageCompressorServiceTests()
    {
        _optionsMock = Substitute.For<IOptions<ImageProcessingOptions>>();
        
        _optionsMock.Value.Returns(new ImageProcessingOptions()
        {
            JpegQuality = 80,
            PngCompressionLevel = 6
        });
        
        _service = new ImageCompressorService(_optionsMock);
    }

    #region Compress

    [Fact]
    public async Task Compress_ShouldReturnJpegStream_WhenFormatIsJpeg()
    {
        // Arrange
        await using MemoryStream sourceStream = CreateJpegImage();

        // Act
        await using Stream result = await _service.Compress(
            sourceStream,
            ImageFormat.Jpeg);

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        result.Position.ShouldBe(0);

        result.CanRead.ShouldBeTrue();

        using Image image = await Image.LoadAsync(result);

        image.ShouldNotBeNull();
        image.Metadata.DecodedImageFormat.ShouldBe(SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance);
    }

    [Fact]
    public async Task Compress_ShouldReturnPngStream_WhenFormatIsPng()
    {
        // Arrange
        await using MemoryStream sourceStream = CreateJpegImage();

        // Act
        await using Stream result = await _service.Compress(
            sourceStream,
            ImageFormat.Png);

        // Assert
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        result.Position.ShouldBe(0);

        result.CanRead.ShouldBeTrue();

        using Image image = await Image.LoadAsync(result);

        image.ShouldNotBeNull();
        image.Metadata.DecodedImageFormat.ShouldBe(SixLabors.ImageSharp.Formats.Png.PngFormat.Instance);
    }

    [Fact]
    public async Task Compress_ShouldThrowArgumentOutOfRangeException_WhenFormatIsUnsupported()
    {
        // Arrange
        await using MemoryStream sourceStream = CreateJpegImage();

        ImageFormat format = (ImageFormat)999;

        // Act
        Func<Task> action = () => _service.Compress(sourceStream, format);

        // Assert
        ArgumentOutOfRangeException exception = await Should.ThrowAsync<ArgumentOutOfRangeException>(action);

        exception.ParamName.ShouldBe(nameof(format));
        exception.ActualValue.ShouldBe(format);
    }

    #endregion
    
    #region Helpers
    
    private MemoryStream CreateJpegImage()
    {
        MemoryStream stream = new();
        using Image<Rgba32> image = new(100, 100);
        image.SaveAsJpeg(stream, new JpegEncoder() { Quality = _optionsMock.Value.JpegQuality });
        stream.Position = 0;

        return stream;
    }
    
    #endregion
}