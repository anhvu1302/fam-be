using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FAM.Application.Auth.DTOs;
using FluentAssertions;
using Xunit;

namespace FAM.IntegrationTests.Auth;

/// <summary>
/// Integration tests for Auth API endpoints
/// </summary>
public class AuthApiTests : IClassFixture<FamWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly FamWebApplicationFactory _factory;

    public AuthApiTests(FamWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Reset admin user to original state before running tests
        // This ensures password is "Admin@123" and lockout is cleared
        await _factory.ResetAdminUserAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginRequest = new
        {
            username = "admin",
            password = "Admin@123",
            rememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.User.Should().NotBeNull();
        content.User.Username.Should().Be("admin");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            username = "admin",
            password = "WrongPassword",
            rememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingFields_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            username = "admin"
            // Missing password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange - First login to get refresh token
        var loginResponse = await LoginAsAdmin();

        var refreshRequest = new
        {
            refreshToken = loginResponse.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.AccessToken.Should().NotBe(loginResponse.AccessToken); // New token should be different
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new
        {
            refreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Logged out successfully");
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout All Tests

    [Fact]
    public async Task LogoutAll_WithExceptCurrentDevice_ShouldReturnSuccess()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            exceptCurrentDevice = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout-all", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Logged out from all devices successfully");
    }

    [Fact]
    public async Task LogoutAll_WithoutExceptCurrentDevice_ShouldLogoutAllDevices()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            exceptCurrentDevice = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout-all", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            currentPassword = "Admin@123",
            newPassword = "NewAdmin@456",
            logoutAllDevices = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password changed successfully");

        // Cleanup - Change back to original password
        var changeBackRequest = new
        {
            currentPassword = "NewAdmin@456",
            newPassword = "Admin@123",
            logoutAllDevices = false
        };
        await _client.PostAsJsonAsync("/api/auth/change-password", changeBackRequest);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidCurrentPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            currentPassword = "WrongPassword",
            newPassword = "NewAdmin@456",
            logoutAllDevices = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithLogoutAllDevices_ShouldLogoutOtherDevices()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            currentPassword = "Admin@123",
            newPassword = "NewAdmin@456",
            logoutAllDevices = true // Should logout all except current
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        var changeBackRequest = new
        {
            currentPassword = "NewAdmin@456",
            newPassword = "Admin@123",
            logoutAllDevices = false
        };
        await _client.PostAsJsonAsync("/api/auth/change-password", changeBackRequest);
    }

    #endregion

    #region 2FA Tests

    [Fact]
    public async Task Enable2FA_WithValidPassword_ShouldReturnQRCode()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            password = "Admin@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/enable-2fa", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<Enable2FAResponse>();
        content.Should().NotBeNull();
        content!.Secret.Should().NotBeNullOrEmpty();
        content.QrCodeUri.Should().NotBeNullOrEmpty();
        content.QrCodeUri.Should().Contain("otpauth://totp/");
    }

    [Fact]
    public async Task Enable2FA_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/enable-2fa", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Disable2FA_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var request = new
        {
            password = "Admin@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/disable-2fa", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Get Current User Tests

    [Fact]
    public async Task GetMe_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var loginResponse = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("admin");
        content.Should().Contain("userId");
        content.Should().Contain("username");
    }

    [Fact]
    public async Task GetMe_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Helper Methods

    private async Task<LoginResponse> LoginAsAdmin()
    {
        var loginRequest = new
        {
            username = "admin",
            password = "Admin@123",
            rememberMe = false
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!;
    }

    #endregion
}