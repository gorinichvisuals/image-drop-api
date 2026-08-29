namespace ImageDrop.AWS.S3.Services;

internal sealed class StorageService(
    IAmazonS3 s3Client, 
    IOptions<S3Options> options,
    ILogger<StorageService> logger) : IStorageService
{
    private readonly S3Options _options = options.Value;
    
    public async Task<ApiResult> UploadImage(Stream stream, Guid imageId)
    {
        try
        {
            using TransferUtility transferUtility = new(s3Client);
            
            string key = imageId.ToString();
            
            TransferUtilityUploadRequest request = new()
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = stream,
            };

            await transferUtility.UploadAsync(request);

            return ApiResult.Success(StatusCodeConstants.Ok);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            logger.LogError(amazonS3Exception, "Amazon error occured while uploading image: {ImageId}", imageId);
            
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, amazonS3Exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error occured while uploading image: {ImageId}", imageId);
            
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }

    public async Task<ApiResult<Stream>> DownloadImage(Guid imageId)
    {
        try
        {
            string key = imageId.ToString();

            GetObjectResponse response = await s3Client.GetObjectAsync(_options.BucketName, key);
            
            return ApiResult<Stream>.Success(StatusCodeConstants.Ok, response.ResponseStream);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            logger.LogError(amazonS3Exception, "Amazon error occured while downloading image: {ImageId}", imageId);
            
            return ApiResult<Stream>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, amazonS3Exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error occured while downloading image: {ImageId}", imageId);
            
            return ApiResult<Stream>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }

    public async Task<ApiResult> DeleteImage(Guid imageId)
    {
        try
        {
            string key = imageId.ToString();
            
            await s3Client.DeleteObjectAsync(_options.BucketName, key);

            return ApiResult.Success(StatusCodeConstants.Ok);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            logger.LogError(amazonS3Exception, "Amazon error occured while deleting image: {ImageId}", imageId);
            
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, amazonS3Exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error occured while deleting image: {ImageId}", imageId);
            
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }
    
    public async Task<ApiResult> DeleteImages(ICollection<Guid> imageIds)
    {
        try
        {
            List<KeyVersion> keyObjects = imageIds
                .Select(id => new KeyVersion
                {
                    Key = id.ToString()
                })
                .ToList();

            if (keyObjects.Count is 0)
                return ApiResult.Success(StatusCodeConstants.Ok);

            DeleteObjectsRequest request = new()
            {
                BucketName = _options.BucketName,
                Objects = keyObjects
            };
            
            await s3Client.DeleteObjectsAsync(request);

            return ApiResult.Success(StatusCodeConstants.Ok);
        }
        catch (AmazonS3Exception amazonS3Exception)
        {
            logger.LogError(amazonS3Exception, "Amazon error occured while deleting images: {ImageIds}", imageIds);
            
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, amazonS3Exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error occured while deleting images: {ImageIds}", imageIds);
            
            return ApiResult.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }
}