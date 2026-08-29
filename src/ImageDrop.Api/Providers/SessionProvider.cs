namespace ImageDrop.Api.Providers;

public sealed class SessionProvider(IHttpContextAccessor httpContextAccessor) : ISessionProvider
{
    public string GetUserSessionToken()
    {
        HttpContext context = httpContextAccessor.HttpContext!;

        return context!.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty);
    }
    
    public int? GetUserId()
    {
        ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated is not true)
            return null;

        return int.TryParse(user.FindFirst(UserClaimConstants.Id)?.Value, out int id)
            ? id
            : null;
    }
}