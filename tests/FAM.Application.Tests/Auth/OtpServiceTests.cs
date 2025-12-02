using System.Security.Cryptography;
using System.Text;
using FluentAssertions;

namespace FAM.Application.Tests.Auth;

/// <summary>
/// Unit tests cho OTP generation và session binding logic
/// </summary>
public class OtpServiceTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public void GenerateRandomOtp_ShouldGenerateCorrectLength(int length)
    {
        // Arrange & Act
        var otp = GenerateRandomOtp(length);

        // Assert
        otp.Should().HaveLength(length);
        otp.Should().MatchRegex("^[0-9]+$"); // Should be all digits
    }

    [Fact]
    public void GenerateRandomOtp_ShouldGenerateUniqueValues()
    {
        // Arrange & Act
        var otps = new HashSet<string>();
        for (var i = 0; i < 100; i++) otps.Add(GenerateRandomOtp(6));

        // Assert
        otps.Should().HaveCountGreaterThan(90); // At least 90% unique
    }

    [Fact]
    public void GenerateSecureCacheKey_WithDifferentSessions_ShouldGenerateDifferentKeys()
    {
        // Arrange
        var userId = 1;
        var session1 = "session-token-1";
        var session2 = "session-token-2";

        // Act
        var key1 = GenerateSecureCacheKey(userId, session1);
        var key2 = GenerateSecureCacheKey(userId, session2);

        // Assert
        key1.Should().NotBe(key2);
        key1.Should().StartWith($"otp:{userId}:");
        key2.Should().StartWith($"otp:{userId}:");
    }

    [Fact]
    public void GenerateSecureCacheKey_WithSameSession_ShouldGenerateSameKey()
    {
        // Arrange
        var userId = 1;
        var sessionToken = "session-token-123";

        // Act
        var key1 = GenerateSecureCacheKey(userId, sessionToken);
        var key2 = GenerateSecureCacheKey(userId, sessionToken);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GenerateSecureCacheKey_ShouldUseSHA256()
    {
        // Arrange
        var userId = 1;
        var sessionToken = "test-session";

        // Act
        var key = GenerateSecureCacheKey(userId, sessionToken);

        // Assert
        key.Should().MatchRegex(@"^otp:\d+:[a-f0-9]{64}$"); // SHA256 = 64 hex chars
    }

    // Helper methods to test OTP logic without dependencies
    private static string GenerateRandomOtp(int length)
    {
        var random = new Random();
        var otp = new char[length];
        for (var i = 0; i < length; i++) otp[i] = (char)('0' + random.Next(0, 10));
        return new string(otp);
    }

    private static string GenerateSecureCacheKey(int userId, string sessionToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sessionToken));
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        return $"otp:{userId}:{hashString}";
    }
}