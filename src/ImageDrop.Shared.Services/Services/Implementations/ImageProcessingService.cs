namespace ImageDrop.Shared.Services.Services.Implementations;

internal sealed class ImageProcessingService(
    IImageCompressorService imageCompressorService, 
    IStorageService storageService, 
    IUnitOfWork unitOfWork, 
    IOptions<ImageCleanupOptions> imageCleanupOptions,
    ILogger<ImageProcessingService> logger) : IImageProcessingService
{
    private readonly ImageCleanupOptions _imageCleanupOptions = imageCleanupOptions.Value;
    
    public async Task<bool> ProcessImage(Guid imageId)
    {
        ImageGetFormatDto? imageFromDb = await unitOfWork.ImageRepository
             .GetSelectedItem(image => new ImageGetFormatDto() { Format = image.Format }, image => image.Id == imageId);

        if (imageFromDb is null)
        {
            logger.LogError("Image with Id - {ImageId} could not be found.", imageId);
            
            await unitOfWork.ImageRepository.UpdateImageProcessingStatus(imageId,  ImageProcessingStatus.Failed);
            
            return false;
        }

        ApiResult<Stream> result = await storageService.DownloadImage(imageId);

        if (!result.IsSucceed)
        {
            logger.LogError(result.ErrorMessage);
            
            await unitOfWork.ImageRepository.UpdateImageProcessingStatus(imageId,  ImageProcessingStatus.Failed);

            return false;
        }
        
        Stream sourceStream = result.Data!;
        Stream compressedStream = await imageCompressorService.Compress(sourceStream, imageFromDb.Format);
        
        ApiResult uploadResult = await storageService.UploadImage(compressedStream, imageId);
        
        if (!uploadResult.IsSucceed)
        {
            logger.LogError(uploadResult.ErrorMessage);
            
            await unitOfWork.ImageRepository.UpdateImageProcessingStatus(imageId,  ImageProcessingStatus.Failed);
            
            return false;
        }
        
        await unitOfWork.ImageRepository.UpdateImageProcessingStatus(imageId,  ImageProcessingStatus.Succeed);
        
        logger.LogInformation("Image with Id - {ImageId} processed.", imageId);
        
        return true;
    }

    public async Task ImagesCleanup()
    {
        ICollection<Guid> imageIds = await unitOfWork.ImageRepository.GetSelectedItems(
            image => image.Id, 
            image => image.UserId == null && image.CreatedAt < DateTimeOffset.UtcNow.AddDays(-_imageCleanupOptions.UnownedImageRetentionDays));

        if (imageIds.Count is 0)
        {
            logger.LogInformation("No images to delete found.");
            
            return;
        }

        foreach (ICollection<Guid> batch in imageIds.Chunk(_imageCleanupOptions.MaxDeleteObjectsBatchSize))
            await storageService.DeleteImages(batch);
        
        await unitOfWork.ImageRepository.Delete(image => imageIds.Contains(image.Id));
        
        logger.LogInformation("Images deleted.");
    }
}