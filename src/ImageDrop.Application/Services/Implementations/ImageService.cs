namespace ImageDrop.Application.Services.Implementations;

internal sealed class ImageService(
    IUnitOfWork unitOfWork, 
    IStorageService storageService, 
    ISqsPublisher sqsPublisher,
    IOptions<S3Options> options) : IImageService
{
    public async Task<ApiResult<ICollection<ImageGetDto>>> GetUserImages(int? userId, CancellationToken cancellationToken)
    {
        try
        {
            ICollection<ImageGetDto> images = await unitOfWork.ImageRepository
                .GetSelectedItems(
                    image => new ImageGetDto() { ImageId = image.Id }, 
                    image => image.UserId == userId,
                    image => image.CreatedAt,
                    cancellationToken);
            
            foreach(ImageGetDto image in images)
                image.Url = GetImageUrl(image.ImageId);
            
            return ApiResult<ICollection<ImageGetDto>>.Success(StatusCodeConstants.Ok, images);
        }
        catch (Exception exception)
        {
            return ApiResult<ICollection<ImageGetDto>>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }

    public async Task<ApiResult<ImageGetDto>> GetImageById(Guid imageId, CancellationToken cancellationToken)
    {
        try
        {
            ImageGetDto? image = await unitOfWork.ImageRepository
                .GetSelectedItem(image => new  ImageGetDto() { ImageId = image.Id }, image => image.Id == imageId, cancellationToken);
            
            if(image is null)
                return ApiResult<ImageGetDto>.Fail(StatusCodeConstants.NotFound, ErrorStatusCode.IMAGE_NOT_FOUND, "Image not found.");
            
            image.Url = GetImageUrl(image.ImageId);
            
            return ApiResult<ImageGetDto>.Success(StatusCodeConstants.Ok, image);
        }
        catch (Exception exception)
        {
            return ApiResult<ImageGetDto>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }
    
    public async Task<ApiResult<ImageGetDto>> UploadImage(Stream stream, ImageFormat format, int? userId = null)
    {
        try
        {
            Image newImage = new()
            {
                UserId = userId,
                Format = format,
            };

            await unitOfWork.ImageRepository.Add(newImage);
            await unitOfWork.Save();

            ApiResult storageResult = await storageService.UploadImage(stream, newImage.Id);
            
            if(!storageResult.IsSucceed)
                return ApiResult<ImageGetDto>.Fail(storageResult.StatusCode, ErrorStatusCode.INTERNAL_SERVER_ERROR,  storageResult.ErrorMessage!);

            await PublishToQueue(newImage.Id);

            ImageGetDto dto = new()
            {
                ImageId = newImage.Id,
                Url = GetImageUrl(newImage.Id)
            };
            
            return ApiResult<ImageGetDto>.Success(StatusCodeConstants.Created, dto);
        }
        catch (Exception exception)
        {
            return ApiResult<ImageGetDto>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }

    public async Task<ApiResult> DeleteImage(Guid imageId, int? userId)
    {
        try
        {
            bool isImageExists = await unitOfWork.ImageRepository
                .Any(image => image.Id == imageId && image.UserId == userId && image.ProcessingStatus != ImageProcessingStatus.Processing);
            
            if(!isImageExists)
                return ApiResult.Fail(StatusCodeConstants.NotFound, ErrorStatusCode.IMAGE_NOT_FOUND,"Image not found, in processing or already deleted.");
            
            ApiResult storageResult = await storageService.DeleteImage(imageId);
            
            if(!storageResult.IsSucceed)
                return storageResult;
            
            await unitOfWork.ImageRepository.Delete(image => image.Id == imageId);
            
            return ApiResult.Success(StatusCodeConstants.NoContent);
        }
        catch (Exception exception)
        {
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }

    private string GetImageUrl(Guid imageId) => $"{options.Value.PublicBaseUrl}/{imageId}";
    
    private async Task PublishToQueue(Guid imageId)
    {
        ImageDropCreateImage message =  new () { ImageId = imageId };
        
        await sqsPublisher.PublishToImageDropQueue(message, MessageGroupIdConstants.ImageDropGroupId);
    }
}