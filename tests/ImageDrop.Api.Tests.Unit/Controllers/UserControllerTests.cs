namespace ImageDrop.Api.Tests.Unit.Controllers;

public sealed class UserControllerTests
{
    private readonly IUserService _userServiceMock;
    
    private readonly UserController _controller;
    
    public UserControllerTests()
    {
        _userServiceMock = Substitute.For<IUserService>();
        _controller = new UserController(_userServiceMock);
    }
    
    #region CreateUser

    [Fact]
    public async Task CreateUser_ShouldReturnCreated_WhenServiceSucceeds()
    {
        // Arrange
        UserCreateDto userCreateDto = new()
        {
            Nickname = "nickname",
            Email = "email@email.com",
            Password = "password",
        };

        AuthDto authDto = new();

        ApiResult<AuthDto> expectedResult = ApiResult<AuthDto>.Success(
            StatusCodeConstants.Created,
            authDto);

        _userServiceMock
            .CreateUser(userCreateDto)
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.CreateUser(userCreateDto);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.Created);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _userServiceMock
            .Received(1)
            .CreateUser(userCreateDto);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        UserCreateDto userCreateDto = new()        
        {
            Nickname = "nickname",
            Email = "email@email.com",
            Password = "password",
        };

        ApiResult<AuthDto> expectedResult = ApiResult<AuthDto>.Fail(
            StatusCodeConstants.InternalServerError,
            ErrorStatusCode.INTERNAL_SERVER_ERROR,
            "Something went wrong");

        _userServiceMock
            .CreateUser(userCreateDto)
            .Returns(expectedResult);

        // Act
        IActionResult result = await _controller.CreateUser(userCreateDto);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();

        objectResult.StatusCode.ShouldBe(StatusCodeConstants.InternalServerError);
        objectResult.Value.ShouldBeSameAs(expectedResult);

        await _userServiceMock
            .Received(1)
            .CreateUser(userCreateDto);
    }

    #endregion
}