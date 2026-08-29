namespace ImageDrop.Api.Controllers;

[ApiController]
[Route("api/users")]
[EnableRateLimiting(RateLimiterPolicyConstants.DefaultPolicy)]
public class UserController(IUserService userService) : Controller
{
    [HttpPost("create")]
    [SwaggerResponse(StatusCodeConstants.Created, Type = typeof(SuccessSwaggerModel<AuthDto>))]
    [SwaggerResponse(StatusCodeConstants.BadRequest, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
    {
        ApiResult<AuthDto> result = await userService.CreateUser(userCreateDto);
        
        return StatusCode(result.StatusCode, result);
    }
}