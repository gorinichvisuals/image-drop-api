namespace ImageDrop.Application.Tests.Unit.Services;

public sealed class CacheServiceTests
{    
    private readonly IDistributedCache _cacheMock;

    private readonly CacheService _service;
    
    public CacheServiceTests()
    {
        _cacheMock = Substitute.For<IDistributedCache>();
        _service = new CacheService(_cacheMock);
    }

    #region TryGet
    
    [Fact]
    public async Task TryGet_ShouldReturnFalse_WhenValueDoesNotExist()
    {
        // Arrange
        const string key = "test-key";

        _cacheMock
            .GetAsync(key, CancellationToken.None)
            .Returns(Task.FromResult<byte[]?>(null));

        // Act
        (bool itemExists, string? value) = await _service.TryGet<string>(key);

        // Assert
        itemExists.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public async Task TryGet_ShouldReturnFalse_WhenValueIsEmpty()
    {
        // Arrange
        const string key = "test-key";

        _cacheMock
            .GetAsync(key, CancellationToken.None)
            .Returns(Task.FromResult(Array.Empty<byte>()));

        // Act
        (bool itemExists, string? value) = await _service.TryGet<string>(key);

        // Assert
        itemExists.ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public async Task TryGet_ShouldReturnValue_WhenValueIsValid()
    {
        // Arrange
        const string key = "test-key";
        const string expectedValue = "test-value";

        string serializedValue = JsonSerializer.Serialize(expectedValue);
        byte[] cachedValue = Encoding.UTF8.GetBytes(serializedValue);

        _cacheMock
            .GetAsync(key, CancellationToken.None)
            .Returns(Task.FromResult<byte[]?>(cachedValue));

        // Act
        (bool itemExists, string? value) = await _service.TryGet<string>(key);

        // Assert
        itemExists.ShouldBeTrue();
        value.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task TryGet_ShouldReturnFalse_WhenValueIsInvalid()
    {
        // Arrange
        const string key = "test-key";

        byte[] cachedValue = "invalid-json"u8.ToArray();

        _cacheMock
            .GetAsync(key, CancellationToken.None)
            .Returns(Task.FromResult<byte[]?>(cachedValue));

        // Act
        (bool itemExists, string? value) = await _service.TryGet<string>(key);

        // Assert
        itemExists.ShouldBeFalse();
        value.ShouldBeNull();
    }
    
    #endregion
    
    #region Set
    
    [Fact]
    public async Task Set_ShouldSetValue()
    {
        // Arrange
        const string key = "test-key";
        const string value = "test-value";

        // Act
        await _service.Set(key, value);

        // Assert
        await _cacheMock.Received(1).SetAsync(
            key,
            Arg.Any<byte[]>(),
            Arg.Is<DistributedCacheEntryOptions>(options =>
                options.AbsoluteExpirationRelativeToNow == TimeSpan.FromDays(1)),
            CancellationToken.None);
    }
    
    #endregion
}