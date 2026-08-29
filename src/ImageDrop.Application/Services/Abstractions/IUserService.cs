namespace ImageDrop.Application.Services.Abstractions;

public interface IUserService
{
    Task<ApiResult<AuthDto>> CreateUser(UserCreateDto userCreateDto);
}