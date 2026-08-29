namespace ImageDrop.Application.Services.Implementations;

internal sealed class UserService(IJwtService jwtService, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<ApiResult<AuthDto>> CreateUser(UserCreateDto userCreateDto)
    {
        try
        {
            bool isUserExists = await unitOfWork.UserRepository.Any(user => user.Email == userCreateDto.Email || user.Nickname == userCreateDto.Nickname);
            
            if (isUserExists)
                return ApiResult<AuthDto>.Fail(StatusCodeConstants.BadRequest, ErrorStatusCode.USER_ALREADY_EXISTS, "User with this email or nickname already exists");

            User newUser = new()
            {
                Email = userCreateDto.Email,
                Password = Argon2.Hash(userCreateDto.Password),
                Nickname = userCreateDto.Nickname
            };
            
            await unitOfWork.UserRepository.Add(newUser);
            await unitOfWork.Save();
            
            AuthDto authenticationResult = await jwtService.GenerateTokens(newUser, userCreateDto.StaySignedIn);
            
            return ApiResult<AuthDto>.Success(StatusCodeConstants.Ok, authenticationResult);
        }
        catch (Exception exception)
        {
            return ApiResult<AuthDto>.Fail(StatusCodeConstants.InternalServerError, ErrorStatusCode.INTERNAL_SERVER_ERROR, exception.Message);
        }
    }
}