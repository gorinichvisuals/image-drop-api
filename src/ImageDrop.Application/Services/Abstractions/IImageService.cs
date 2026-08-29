namespace ImageDrop.Application.Services.Abstractions;

public interface IImageService
{
    Task<ApiResult<ICollection<ImageGetDto>>> GetUserImages(int? userId, CancellationToken cancellationToken);
    Task<ApiResult<ImageGetDto>> GetImageById(Guid imageId, CancellationToken cancellationToken);
    Task<ApiResult<ImageGetDto>> UploadImage(Stream stream,ImageFormat format, int? userId = null);
    Task<ApiResult> DeleteImage(Guid imageId, int? userId);
}