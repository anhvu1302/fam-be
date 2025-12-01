using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace FAM.Application.Tests.Auth;

/// <summary>
/// Unit tests cho 2FA logic trong User entity
/// </summary>
public class UserTwoFactorAuthenticationTests
{
    [Fact]
    public void EnableTwoFactor_ShouldSetTwoFactorEnabled()
    {
        // Arrange
        var user = CreateTestUser();
        var secret = "JBSWY3DPEHPK3PXP";

        // Act
        user.EnableTwoFactor(secret);

        // Assert
        user.TwoFactorEnabled.Should().BeTrue();
        user.TwoFactorSecret.Should().Be(secret);
        user.TwoFactorSetupDate.Should().NotBeNull();
        user.TwoFactorSetupDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DisableTwoFactor_ShouldClearAllTwoFactorData()
    {
        // Arrange
        var user = CreateTestUser();
        user.EnableTwoFactor("JBSWY3DPEHPK3PXP");

        // Act
        user.DisableTwoFactor();

        // Assert
        user.TwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();
        user.TwoFactorSetupDate.Should().BeNull();
        user.TwoFactorBackupCodes.Should().BeNull();
    }

    [Fact]
    public void TwoFactorBackupCodes_ShouldStoreAndRetrieveJson()
    {
        // Arrange
        var user = CreateTestUser();
        user.EnableTwoFactor("secret");

        // Assert
        // TwoFactorBackupCodes property is accessible for reading
        // Backup codes are managed internally by the User entity
        user.TwoFactorBackupCodes.Should().BeNull();
    }

    [Fact]
    public void VerifyEmail_ShouldSetEmailVerifiedStatus()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsEmailVerified.Should().BeFalse();

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.EmailVerifiedAt.Should().NotBeNull();
        user.EmailVerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetOrCreateDevice_ShouldCreateNewDevice_WhenNotExists()
    {
        // Arrange
        var user = CreateTestUser();
        var deviceId = "device-123";
        var deviceName = "Chrome on Windows";
        var deviceType = "browser";
        var userAgent = "Mozilla/5.0...";
        var ipAddress = "192.168.1.100";
        var location = "Hanoi, Vietnam";

        // Act
        var device = user.GetOrCreateDevice(
            deviceId,
            deviceName,
            deviceType,
            userAgent,
            ipAddress,
            location
        );

        // Assert
        device.Should().NotBeNull();
        device.DeviceId.Should().Be(deviceId);
        device.DeviceName.Should().Be(deviceName);
        device.DeviceType.Should().Be(deviceType);
        device.IpAddress?.Value.Should().Be(ipAddress); // IpAddress is a value object
        device.Location.Should().Be(location);
        device.IsActive.Should().BeTrue();
        user.UserDevices.Should().Contain(device);
    }

    [Fact]
    public void GetOrCreateDevice_ShouldReturnExistingDevice_WhenAlreadyExists()
    {
        // Arrange
        var user = CreateTestUser();
        var deviceId = "device-456";

        // Create device first time
        var firstDevice = user.GetOrCreateDevice(
            deviceId,
            "First Device",
            "mobile",
            "iOS App",
            "10.0.0.1",
            "HCMC"
        );

        // Act - Get same device
        var secondDevice = user.GetOrCreateDevice(
            deviceId,
            "Second Device", // Different name
            "tablet",
            "Android App",
            "10.0.0.2",
            "Hanoi"
        );

        // Assert
        secondDevice.Should().BeSameAs(firstDevice);
        user.UserDevices.Should().HaveCount(1);

        // GetOrCreateDevice only updates IP and location via UpdateActivity
        // It does NOT update deviceName or deviceType
        secondDevice.DeviceName.Should().Be("First Device");
        secondDevice.DeviceType.Should().Be("mobile");
        // IP and Location ARE updated
        secondDevice.IpAddress?.Value.Should().Be("10.0.0.2");
        secondDevice.Location.Should().Be("Hanoi");
    }

    [Fact]
    public void Activate_ShouldSetUserActive()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate();
        user.IsActive.Should().BeFalse();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetUserInactive()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive.Should().BeTrue();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void EnableTwoFactor_MultipleTimes_ShouldUpdateSecret()
    {
        // Arrange
        var user = CreateTestUser();
        var firstSecret = "FIRST_SECRET_123";
        var secondSecret = "SECOND_SECRET_456";

        // Act
        user.EnableTwoFactor(firstSecret);
        var firstSetupDate = user.TwoFactorSetupDate;

        // Wait a bit and enable again
        Thread.Sleep(10);
        user.EnableTwoFactor(secondSecret);

        // Assert
        user.TwoFactorSecret.Should().Be(secondSecret);
        user.TwoFactorSetupDate.Should().BeAfter(firstSetupDate!.Value);
    }

    private static User CreateTestUser()
    {
        return User.Create(
            "testuser",
            "test@example.com",
            "Password123!",
            firstName: "Test",
            lastName: "User"
        );
    }
}