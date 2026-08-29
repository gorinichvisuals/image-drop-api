namespace ImageDrop.Api.Controllers;

[ApiController]
[Route("api/images")]
[EnableRateLimiting(RateLimiterPolicyConstants.DefaultPolicy)]
public class ImageController(IImageService imageService, ISessionProvider sessionProvider) : Controller
{
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.BasicUser))]
    [SwaggerResponse(StatusCodeConstants.Ok)]
    [SwaggerResponse(StatusCodeConstants.Unauthorized, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.Forbidden, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> GetUserImages(CancellationToken cancellationToken)
    {        
        int? userId = sessionProvider.GetUserId();

        ApiResult result = await imageService.GetUserImages(userId, cancellationToken);
        
        return StatusCode(result.StatusCode, result);
    }
    
    [HttpGet("{imageId}")]
    [SwaggerResponse(StatusCodeConstants.Ok)]
    [SwaggerResponse(StatusCodeConstants.NotFound, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> GetImageById(Guid imageId, CancellationToken cancellationToken)
    {
        ApiResult result = await imageService.GetImageById(imageId, cancellationToken);
        
        return StatusCode(result.StatusCode, result);
    }
    
    [HttpPost]
    [ValidateImageFile]
    [SwaggerResponse(StatusCodeConstants.Created, Type = typeof(SuccessSwaggerModel<ImageGetDto>))]
    [SwaggerResponse(StatusCodeConstants.BadRequest, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> CreateImage(IFormFile file)
    {
        int? userId = sessionProvider.GetUserId();
        await using Stream stream = file.OpenReadStream();
        ImageFormat format = file.ContentType.GetImageFormat();
        
        ApiResult<ImageGetDto> result = await imageService.UploadImage(stream, format, userId);
        
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{imageId}")]
    [Authorize(Roles = nameof(UserRole.BasicUser))]
    [SwaggerResponse(StatusCodeConstants.NoContent)]
    [SwaggerResponse(StatusCodeConstants.Unauthorized, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.Forbidden, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.NotFound, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        int? userId = sessionProvider.GetUserId();
        
        ApiResult result = await imageService.DeleteImage(imageId, userId);
        
        return result.IsSucceed 
            ? NoContent() 
            : StatusCode(result.StatusCode, result);
    }
}