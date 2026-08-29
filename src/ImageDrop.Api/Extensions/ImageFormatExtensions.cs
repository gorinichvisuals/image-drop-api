namespace ImageDrop.Api.Extensions;

public static class ImageFormatExtensions
{
    public static string GetContentType(this ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => "image/jpeg",
            ImageFormat.Png => "image/png",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public static ImageFormat GetImageFormat(this string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ImageFormat.Jpeg,
            "image/png" => ImageFormat.Png,
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null)
        };
    }
}