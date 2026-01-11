using FAM.Domain.Abstractions.Cache;
using FAM.Domain.Abstractions.Storage;
using FAM.Infrastructure.Providers.RateLimit;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace FAM.Infrastructure.Tests.Providers.RateLimit;

/// <summary>
/// Unit tests for RateLimiterStore
/// </summary>
public class RateLimiterStoreTests
{
    private readonly Mock<ICacheProvider> _mockCacheProvider;
    private readonly Mock<ILogger<RateLimiterStore>> _mockLogger;
    private readonly RateLimiterStore _rateLimiterStore;

    public RateLimiterStoreTests()
    {
        _mockCacheProvider = new Mock<ICacheProvider>();
        _mockLogger = new Mock<ILogger<RateLimiterStore>>();
        _rateLimiterStore = new RateLimiterStore(_mockCacheProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task TryAcquireAsync_WhenUnderLimit_ShouldReturnTrue()
    {
        // Arrange
        var key = "test-ip";
        var permitLimit = 100;
        var window = TimeSpan.FromMinutes(1);

        // Mock cache: first call returns null (no existing count), set should succeed
        _mockCacheProvider
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        _mockCacheProvider
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _rateLimiterStore.TryAcquireAsync(key, permitLimit, window);

        // Assert
        Assert.True(result);
        _mockCacheProvider.Verify(
            x => x.SetAsync(It.IsAny<string>(), "1", window, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryAcquireAsync_WhenAtLimit_ShouldReturnFalse()
    {
        // Arrange
        var key = "test-ip";
        var permitLimit = 5;
        var window = TimeSpan.FromMinutes(1);

        // Mock cache: return "5" (at limit)
        _mockCacheProvider
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("5");

        // Act
        var result = await _rateLimiterStore.TryAcquireAsync(key, permitLimit, window);

        // Assert
        Assert.False(result);
        // Should NOT call SetAsync when at limit
        _mockCacheProvider.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryAcquireAsync_MultipleRequests_ShouldIncrementCounter()
    {
        // Arrange
        var key = "test-ip";
        var permitLimit = 100;
        var window = TimeSpan.FromMinutes(1);

        var callCount = 0;
        _mockCacheProvider
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callCount++)
            .ReturnsAsync(() => callCount.ToString());

        _mockCacheProvider
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result1 = await _rateLimiterStore.TryAcquireAsync(key, permitLimit, window);
        var result2 = await _rateLimiterStore.TryAcquireAsync(key, permitLimit, window);
        var result3 = await _rateLimiterStore.TryAcquireAsync(key, permitLimit, window);

        // Assert
        Assert.All(new[] { result1, result2, result3 }, r => Assert.True(r));
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnCurrentCount()
    {
        // Arrange
        var key = "test-ip";

        _mockCacheProvider
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("42");

        // Act
        var count = await _rateLimiterStore.GetCountAsync(key);

        // Assert
        Assert.Equal(42, count);
    }

    [Fact]
    public async Task GetRemainingAsync_ShouldReturnRemainingRequests()
    {
        // Arrange
        var key = "test-ip";
        var permitLimit = 100;

        _mockCacheProvider
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("30");

        // Act
        var remaining = await _rateLimiterStore.GetRemainingAsync(key, permitLimit);

        // Assert
        Assert.Equal(70, remaining); // 100 - 30
    }

    [Fact]
    public async Task ResetAsync_ShouldClearCounter()
    {
        // Arrange
        var key = "test-ip";

        _mockCacheProvider
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _rateLimiterStore.ResetAsync(key);

        // Assert
        Assert.True(result);
        _mockCacheProvider.Verify(
            x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}

/// <summary>
/// Unit tests for AdvancedRateLimiterService
/// </summary>
public class AdvancedRateLimiterServiceTests
{
    private readonly Mock<IRateLimiterStore> _mockStore;
    private readonly Mock<ILogger<AdvancedRateLimiterService>> _mockLogger;
    private readonly AdvancedRateLimiterService _rateLimiterService;

    public AdvancedRateLimiterServiceTests()
    {
        _mockStore = new Mock<IRateLimiterStore>();
        _mockLogger = new Mock<ILogger<AdvancedRateLimiterService>>();
        _rateLimiterService = new AdvancedRateLimiterService(_mockStore.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CheckUserLimitAsync_WhenAllowed_ShouldReturnAllowedResult()
    {
        // Arrange
        var userId = 123L;
        var permitLimit = 100;

        _mockStore
            .Setup(x => x.TryAcquireAsync(It.IsAny<string>(), permitLimit, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockStore
            .Setup(x => x.GetCountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(45);

        // Act
        var result = await _rateLimiterService.CheckUserLimitAsync(userId, permitLimit);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(55, result.RemainingRequests); // 100 - 45
    }

    [Fact]
    public async Task CheckUserLimitAsync_WhenExceeded_ShouldReturnExceededResult()
    {
        // Arrange
        var userId = 123L;
        var permitLimit = 100;

        _mockStore
            .Setup(x => x.TryAcquireAsync(It.IsAny<string>(), permitLimit, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockStore
            .Setup(x => x.GetRemainingAsync(It.IsAny<string>(), permitLimit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mockStore
            .Setup(x => x.GetRetryAfterSecondsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(60);

        // Act
        var result = await _rateLimiterService.CheckUserLimitAsync(userId, permitLimit);

        // Assert
        Assert.False(result.IsAllowed);
        Assert.Equal(0, result.RemainingRequests);
        Assert.Equal(60, result.RetryAfterSeconds);
    }

    [Fact]
    public async Task CheckEndpointLimitAsync_ShouldUseEndpointSpecificKey()
    {
        // Arrange
        var userId = 123L;
        var endpoint = "bulk-import";

        _mockStore
            .Setup(x => x.TryAcquireAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockStore
            .Setup(x => x.GetCountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _rateLimiterService.CheckEndpointLimitAsync(userId, endpoint);

        // Assert - Verify correct key format was used
        _mockStore.Verify(
            x => x.TryAcquireAsync(
                It.Is<string>(k => k.Contains($"endpoint:{endpoint}") && k.Contains($"user:{userId}")),
                It.IsAny<int>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckSensitiveOperationAsync_ShouldLimitLoginAttempts()
    {
        // Arrange
        var identifier = "192.168.1.1";
        var operation = "login";
        var permitLimit = 5;

        _mockStore
            .Setup(x => x.TryAcquireAsync(It.IsAny<string>(), permitLimit, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockStore
            .Setup(x => x.GetCountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _rateLimiterService.CheckSensitiveOperationAsync(identifier, operation, permitLimit);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Equal(3, result.RemainingRequests); // 5 - 2
    }

    [Fact]
    public async Task GetUsageAsync_ShouldReturnUsageStats()
    {
        // Arrange
        var key = "user:123";
        var permitLimit = 100;

        _mockStore
            .Setup(x => x.GetCountAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(75);

        _mockStore
            .Setup(x => x.GetRemainingAsync(key, permitLimit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(25);

        _mockStore
            .Setup(x => x.GetRetryAfterSecondsAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(120);

        // Act
        var usage = await _rateLimiterService.GetUsageAsync(key, permitLimit);

        // Assert
        Assert.Equal(100, usage.TotalLimit);
        Assert.Equal(75, usage.CurrentCount);
        Assert.Equal(25, usage.RemainingRequests);
        Assert.Equal(120, usage.RetryAfterSeconds);
        Assert.False(usage.IsExceeded);
        Assert.Equal(75.0, usage.UsagePercentage);
    }
}

/// <summary>
/// Integration tests for rate limiting with real Redis (optional, requires Redis running)
/// </summary>
public class RateLimitIntegrationTests
{
    // These tests would use a real Redis instance
    // Run with: dotnet test --filter "Category=Integration"

    [Fact(Skip = "Integration test - requires Redis")]
    public async Task RateLimiter_MultipleInstances_ShouldShareState()
    {
        // This test would verify that multiple instances share the same rate limit state
        // by creating multiple RateLimiterStore instances with the same Redis connection
    }

    [Fact(Skip = "Integration test - requires Redis")]
    public async Task RateLimiter_WindowExpiration_ShouldResetCounter()
    {
        // This test would verify that rate limit counters are properly reset after the window expires
    }
}
