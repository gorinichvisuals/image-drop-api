namespace ImageDrop.Application.Tests.Unit.Services;

public sealed class UserServiceTests
{
    private readonly IJwtService _jwtServiceMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    private readonly UserService _service;

    public UserServiceTests()
    {
        _jwtServiceMock = Substitute.For<IJwtService>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _service = new UserService(
            _jwtServiceMock,
            _unitOfWorkMock);
    }

    #region CreateUser

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenUserAlreadyExists()
    {
        // Arrange
        UserCreateDto userCreateDto = new()
        {
            Email = "test@test.com",
            Nickname = "test",
            Password = "Password123"
        };

        _unitOfWorkMock.UserRepository
            .Any(Arg.Any<Expression<Func<User, bool>>>())
            .Returns(true);

        // Act
        ApiResult<AuthDto> result = await _service.CreateUser(userCreateDto);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.BadRequest);
        result.ErrorCode.ShouldBe(ErrorStatusCode.USER_ALREADY_EXISTS.ToString());
        result.ErrorMessage.ShouldBe(
            "User with this email or nickname already exists");

        await _unitOfWorkMock.UserRepository
            .DidNotReceive()
            .Add(Arg.Any<User>());

        await _unitOfWorkMock
            .DidNotReceive()
            .Save();

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(
                Arg.Any<User>(),
                Arg.Any<bool>());
    }

    [Fact]
    public async Task CreateUser_ShouldCreateUserAndReturnOk_WhenUserDoesNotExist()
    {
        // Arrange
        UserCreateDto userCreateDto = new()
        {
            Email = "test@test.com",
            Nickname = "test",
            Password = "Password123",
            StaySignedIn = true
        };

        AuthDto authenticationResult = new();

        _unitOfWorkMock.UserRepository
            .Any(Arg.Any<Expression<Func<User, bool>>>())
            .Returns(false);

        _jwtServiceMock
            .GenerateTokens(
                Arg.Any<User>(),
                userCreateDto.StaySignedIn)
            .Returns(authenticationResult);

        // Act
        ApiResult<AuthDto> result = await _service.CreateUser(userCreateDto);

        // Assert
        result.IsSucceed.ShouldBeTrue();
        result.StatusCode.ShouldBe(StatusCodeConstants.Ok);
        result.Data.ShouldBeSameAs(authenticationResult);

        await _unitOfWorkMock.UserRepository
            .Received(1)
            .Add(Arg.Any<User>());

        await _unitOfWorkMock
            .Received(1)
            .Save();

        await _jwtServiceMock
            .Received(1)
            .GenerateTokens(
                Arg.Any<User>(),
                userCreateDto.StaySignedIn);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        UserCreateDto userCreateDto = new()
        {
            Email = "test@test.com",
            Nickname = "test",
            Password = "Password123"
        };

        const string exceptionMessage = "Database error";

        _unitOfWorkMock.UserRepository
            .Any(Arg.Any<Expression<Func<User, bool>>>())
            .Returns<Task<bool>>(_ =>
                throw new Exception(exceptionMessage));

        // Act
        ApiResult<AuthDto> result = await _service.CreateUser(userCreateDto);

        // Assert
        result.IsSucceed.ShouldBeFalse();
        result.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        result.ErrorCode.ShouldBe(
            ErrorStatusCode.INTERNAL_SERVER_ERROR.ToString());
        result.ErrorMessage.ShouldBe(exceptionMessage);

        await _unitOfWorkMock.UserRepository
            .DidNotReceive()
            .Add(Arg.Any<User>());

        await _unitOfWorkMock
            .DidNotReceive()
            .Save();

        await _jwtServiceMock
            .DidNotReceive()
            .GenerateTokens(
                Arg.Any<User>(),
                Arg.Any<bool>());
    }

    #endregion
}