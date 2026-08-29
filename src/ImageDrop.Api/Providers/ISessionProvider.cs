namespace ImageDrop.Api.Providers;

public interface ISessionProvider
{
    string GetUserSessionToken();
    int? GetUserId();
}