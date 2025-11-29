using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FAM.Application.Auth.DTOs;
using FluentAssertions;
using Xunit;

namespace FAM.IntegrationTests.Auth;

/// <summary>
/// Integration tests for JWKS API endpoints
/// </summary>
public class JwksApiTests : IClassFixture<FamWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly FamWebApplicationFactory _factory;
    private string? _adminToken;

    public JwksApiTests(FamWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetAdminUserAsync();
        _adminToken = await GetAdminTokenAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    #region Public JWKS Endpoints

    [Fact]
    public async Task GetJwks_WellKnownEndpoint_ShouldReturnJwks()
    {
        // Act
        var response = await _client.GetAsync("/.well-known/jwks.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jwks = JsonSerializer.Deserialize<JwksDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jwks.Should().NotBeNull();
        jwks!.Keys.Should().NotBeNull();
    }

    [Fact]
    public async Task GetJwks_ApiAuthEndpoint_ShouldReturnJwks()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/jwks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var jwks = JsonSerializer.Deserialize<JwksDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        jwks.Should().NotBeNull();
    }

    [Fact]
    public async Task GetJwks_ShouldBePubliclyAccessible()
    {
        // Act - No authentication header
        var response = await _client.GetAsync("/.well-known/jwks.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Admin Endpoints - Authentication Tests

    [Fact]
    public async Task GetAllKeys_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/signing-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllKeys_WithAdminAuth_ShouldReturnKeys()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        // Act
        var response = await _client.GetAsync("/api/admin/signing-keys");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var keys = await response.Content.ReadFromJsonAsync<List<SigningKeyResponse>>();
        keys.Should().NotBeNull();
    }

    #endregion

    #region Generate Key Tests

    [Fact]
    public async Task GenerateKey_WithValidRequest_ShouldCreateNewKey()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        var request = new GenerateSigningKeyRequest
        {
            Algorithm = "RS256",
            KeySize = 2048,
            Description = "Test key from integration test",
            ActivateImmediately = false // Don't disrupt other tests
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/signing-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var key = await response.Content.ReadFromJsonAsync<SigningKeyResponse>();
        key.Should().NotBeNull();
        key!.Algorithm.Should().Be("RS256");
        key.KeySize.Should().Be(2048);
        key.Description.Should().Contain("Test key");
    }

    [Fact]
    public async Task GenerateKey_WithInvalidAlgorithm_ShouldReturnBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        var request = new GenerateSigningKeyRequest
        {
            Algorithm = "INVALID",
            KeySize = 2048
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/signing-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateKey_WithInvalidKeySize_ShouldReturnBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        var request = new GenerateSigningKeyRequest
        {
            Algorithm = "RS256",
            KeySize = 1024 // Too small
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/signing-keys", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Key Lifecycle Tests

    [Fact]
    public async Task KeyLifecycle_ActivateDeactivateRevoke_ShouldWork()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        // Step 1: Generate a new key (inactive)
        var generateRequest = new GenerateSigningKeyRequest
        {
            Algorithm = "RS256",
            KeySize = 2048,
            ActivateImmediately = false
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/signing-keys", generateRequest);
        generateResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdKey = await generateResponse.Content.ReadFromJsonAsync<SigningKeyResponse>();
        var keyId = createdKey!.Id;

        // Step 2: Activate the key
        var activateResponse = await _client.PostAsync($"/api/admin/signing-keys/{keyId}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify activation
        var getResponse1 = await _client.GetAsync($"/api/admin/signing-keys/{keyId}");
        var activatedKey = await getResponse1.Content.ReadFromJsonAsync<SigningKeyResponse>();
        activatedKey!.IsActive.Should().BeTrue();

        // Step 3: Deactivate the key
        var deactivateResponse = await _client.PostAsync($"/api/admin/signing-keys/{keyId}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deactivation
        var getResponse2 = await _client.GetAsync($"/api/admin/signing-keys/{keyId}");
        var deactivatedKey = await getResponse2.Content.ReadFromJsonAsync<SigningKeyResponse>();
        deactivatedKey!.IsActive.Should().BeFalse();

        // Step 4: Revoke the key
        var revokeRequest = new RevokeSigningKeyRequest { Reason = "Test revocation" };
        var revokeResponse = await _client.PostAsJsonAsync($"/api/admin/signing-keys/{keyId}/revoke", revokeRequest);
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify revocation
        var getResponse3 = await _client.GetAsync($"/api/admin/signing-keys/{keyId}");
        var revokedKey = await getResponse3.Content.ReadFromJsonAsync<SigningKeyResponse>();
        revokedKey!.IsRevoked.Should().BeTrue();
        revokedKey.RevocationReason.Should().Be("Test revocation");

        // Step 5: Delete the revoked key
        var deleteResponse = await _client.DeleteAsync($"/api/admin/signing-keys/{keyId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deletion
        var getResponse4 = await _client.GetAsync($"/api/admin/signing-keys/{keyId}");
        getResponse4.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteKey_WhenNotRevoked_ShouldReturnBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        // Generate a key
        var generateRequest = new GenerateSigningKeyRequest
        {
            Algorithm = "RS256",
            KeySize = 2048,
            ActivateImmediately = false
        };
        var generateResponse = await _client.PostAsJsonAsync("/api/admin/signing-keys", generateRequest);
        var createdKey = await generateResponse.Content.ReadFromJsonAsync<SigningKeyResponse>();

        // Act - Try to delete without revoking first
        var deleteResponse = await _client.DeleteAsync($"/api/admin/signing-keys/{createdKey!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Cleanup: Revoke and delete
        var revokeRequest = new RevokeSigningKeyRequest { Reason = "Cleanup" };
        await _client.PostAsJsonAsync($"/api/admin/signing-keys/{createdKey.Id}/revoke", revokeRequest);
        await _client.DeleteAsync($"/api/admin/signing-keys/{createdKey.Id}");
    }

    #endregion

    #region Rotate Key Tests

    [Fact]
    public async Task RotateKey_ShouldCreateNewActiveKey()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _adminToken);

        var request = new RotateKeyRequest
        {
            Algorithm = "RS256",
            KeySize = 2048,
            Description = "Rotated key",
            RevokeOldKey = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/signing-keys/rotate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newKey = await response.Content.ReadFromJsonAsync<SigningKeyResponse>();
        newKey.Should().NotBeNull();
        newKey!.IsActive.Should().BeTrue();
        newKey.Description.Should().Contain("Rotated key");
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new
        {
            identity = "admin",
            password = "Admin@123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        return json.RootElement.GetProperty("accessToken").GetString()!;
    }

    #endregion
}