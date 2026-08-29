namespace ImageDrop.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ValidateImageFileAttribute : Attribute, IAsyncActionFilter
{
    private static readonly HashSet<string> AllowedContentTypes = Enum.GetValues<ImageFormat>()
        .Select(format => format.GetContentType())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        IFormFile? file = context.ActionArguments.Values.OfType<IFormFile>().FirstOrDefault();

        if (file is null)
        {
            context.Result = new BadRequestObjectResult(
                ApiResult.Fail(StatusCodeConstants.BadRequest, ErrorStatusCode.INVALID_FILE, "File is required."));
            
            return;
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            context.Result = new BadRequestObjectResult(
                ApiResult.Fail(StatusCodeConstants.BadRequest, ErrorStatusCode.INVALID_FILE_TYPE, "File type is not supported."));
            
            return;
        }
        
        await next();
    }
}