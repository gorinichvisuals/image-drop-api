namespace ImageDrop.AWS.S3.Services;

public interface IStorageService
{
    Task<ApiResult> UploadImage(Stream stream, Guid imageId);
    Task<ApiResult<Stream>> DownloadImage(Guid imageId);
    Task<ApiResult> DeleteImage(Guid imageId);
    Task<ApiResult> DeleteImages(ICollection<Guid> imageIds);
}