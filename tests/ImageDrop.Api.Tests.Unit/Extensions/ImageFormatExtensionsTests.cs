namespace ImageDrop.Api.Tests.Unit.Extensions;

public sealed class ImageFormatExtensionsTests
{
    #region GetContentType

    [Fact]
    public void GetContentType_ShouldReturnJpegContentType()
    {
        // Arrange
        ImageFormat format = ImageFormat.Jpeg;

        // Act
        string result = format.GetContentType();

        // Assert
        result.ShouldBe("image/jpeg");
    }

    [Fact]
    public void GetContentType_ShouldReturnPngContentType()
    {
        // Arrange
        ImageFormat format = ImageFormat.Png;

        // Act
        string result = format.GetContentType();

        // Assert
        result.ShouldBe("image/png");
    }

    [Fact]
    public void GetContentType_ShouldThrow_WhenFormatIsNotSupported()
    {
        // Arrange
        ImageFormat format = (ImageFormat)999;

        // Act
        Action action = () => format.GetContentType();

        // Assert
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    #endregion

    #region GetImageFormat

    [Fact]
    public void GetImageFormat_ShouldReturnJpeg_WhenContentTypeIsJpeg()
    {
        // Arrange
        const string contentType = "image/jpeg";

        // Act
        ImageFormat result = contentType.GetImageFormat();

        // Assert
        result.ShouldBe(ImageFormat.Jpeg);
    }

    [Fact]
    public void GetImageFormat_ShouldReturnPng_WhenContentTypeIsPng()
    {
        // Arrange
        const string contentType = "image/png";

        // Act
        ImageFormat result = contentType.GetImageFormat();

        // Assert
        result.ShouldBe(ImageFormat.Png);
    }

    [Fact]
    public void GetImageFormat_ShouldBeCaseInsensitive()
    {
        // Arrange
        const string contentType = "IMAGE/JPEG";

        // Act
        ImageFormat result = contentType.GetImageFormat();

        // Assert
        result.ShouldBe(ImageFormat.Jpeg);
    }

    [Fact]
    public void GetImageFormat_ShouldThrow_WhenContentTypeIsNotSupported()
    {
        // Arrange
        const string contentType = "image/gif";

        // Act
        Action action = () => contentType.GetImageFormat();

        // Assert
        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    #endregion
}