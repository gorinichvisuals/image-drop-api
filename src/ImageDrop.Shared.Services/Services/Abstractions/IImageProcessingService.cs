using ImageDrop.Shared.Constants.Core;

namespace ImageDrop.Shared.Services.Services.Abstractions;

public interface IImageProcessingService
{
    Task<bool> ProcessImage(Guid imageId);
    Task ImagesCleanup();
}