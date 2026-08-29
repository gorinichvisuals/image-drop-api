namespace ImageDrop.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting(RateLimiterPolicyConstants.DefaultPolicy)]
public class AuthController(
    IAuthService authService, 
    ISessionProvider sessionProvider) : Controller
{
    [HttpPost("login")]
    [SwaggerResponse(StatusCodeConstants.Ok, Type = typeof(SuccessSwaggerModel<AuthDto>))]
    [SwaggerResponse(StatusCodeConstants.BadRequest, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.Unauthorized, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        ApiResult<AuthDto> result = await authService.Login(loginDto, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }
    
    [HttpPost("refresh")]
    [Authorize(Roles = DefaultAuthScopeConstants.RefreshTokenRole)]
    [SwaggerResponse(StatusCodeConstants.Ok, Type = typeof(SuccessSwaggerModel<AuthDto>))]
    [SwaggerResponse(StatusCodeConstants.BadRequest, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.Unauthorized, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.TooManyRequests, Type = typeof(ErrorSwaggerModel))]
    [SwaggerResponse(StatusCodeConstants.InternalServerError, Type = typeof(ErrorSwaggerModel))]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        int? userId = sessionProvider.GetUserId();
        string refreshToken = sessionProvider.GetUserSessionToken();

        ApiResult<AuthDto> result = await authService.RefreshToken(userId, refreshToken, cancellationToken);

        return StatusCode(result.StatusCode, result);
    }
}