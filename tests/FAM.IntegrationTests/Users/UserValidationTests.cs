using System.Net;
using System.Net.Http.Json;
using FAM.WebApi.Contracts.Users;
using FluentAssertions;
using Xunit;

namespace FAM.IntegrationTests.Users;

public class UserValidationTests : IClassFixture<FamWebApplicationFactory>
{
    private readonly HttpClient Client;
    private readonly FamWebApplicationFactory _factory;

    public UserValidationTests(FamWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }

    #region Web API Layer Validation (Shape/Format) - 400 BadRequest

    [Fact]
    public async Task CreateUser_WithInvalidEmailFormat_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            "testuser",
            "invalid-email-format", // Invalid email format
            "SecurePass123!",
            "Test",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email"); // ModelState error should mention Email field
    }

    [Fact]
    public async Task CreateUser_WithEmptyUsername_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            "", // Empty username
            "test@example.com",
            "SecurePass123!",
            "Test",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username"); // ModelState error
    }

    [Fact]
    public async Task CreateUser_WithShortPassword_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            "testuser",
            "test@example.com",
            "short", // Too short
            "Test",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password"); // ModelState error
    }

    [Fact]
    public async Task CreateUser_WithInvalidUsernameFormat_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            "invalid@username!", // Contains invalid characters
            "test@example.com",
            "SecurePass123!",
            "Test",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username"); // ModelState error
    }

    [Fact]
    public async Task CreateUser_WithTooLongFirstName_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateUserRequest(
            "testuser",
            "test@example.com",
            "SecurePass123!",
            new string('a', 51), // Exceeds 50 character limit
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FirstName"); // ModelState error
    }

    #endregion

    #region Application Layer Validation (Orchestration) - 409 Conflict

    [Fact]
    public async Task CreateUser_WithDuplicateUsername_ShouldReturn409Conflict()
    {
        // Arrange - Create first user
        var firstRequest = new CreateUserRequest(
            "duplicateuser",
            "first@example.com",
            "SecurePass123!",
            "First",
            "User"
        );

        await Client.PostAsJsonAsync("/api/users", firstRequest);

        // Arrange - Try to create second user with same username
        var secondRequest = new CreateUserRequest(
            "duplicateuser", // Same username
            "second@example.com", // Different email
            "SecurePass123!",
            "Second",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict); // Application layer returns 409

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username is already taken");
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturn409Conflict()
    {
        // Arrange - Create first user
        var firstRequest = new CreateUserRequest(
            "firstuser",
            "duplicate@example.com",
            "SecurePass123!",
            "First",
            "User"
        );

        await Client.PostAsJsonAsync("/api/users", firstRequest);

        // Arrange - Try to create second user with same email
        var secondRequest = new CreateUserRequest(
            "seconduser", // Different username
            "duplicate@example.com", // Same email
            "SecurePass123!",
            "Second",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict); // Application layer returns 409

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email is already taken");
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentUser_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentUserId = 99999L;
        var request = new UpdateUserRequest(
            "newusername",
            "new@example.com",
            "New",
            "Name"
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{nonExistentUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Domain Layer Validation (Business Rules) - DomainException mapped to 422

    [Fact]
    public async Task CreateUser_WithWeakPassword_ShouldReturn422UnprocessableEntity()
    {
        // Arrange
        var request = new CreateUserRequest(
            $"weakpasstest_{Guid.NewGuid():N}",
            $"weakpasstest_{Guid.NewGuid():N}@example.com",
            "weakpass", // Passes Web API min length (8 chars) but ALSO passes DataAnnotations
            // NOTE: Web API only validates min length, not password strength
            // Domain would validate strength, but this password passes Web API layer
            "Test",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        // This password actually passes Web API validation (8 chars minimum)
        // It will fail at Domain layer (Password.Create) due to missing uppercase/number/special char
        // RFC 4918: 422 Unprocessable Entity - semantically incorrect (domain validation failed)
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password"); // Domain validation error
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateUserRequest(
            $"validuser_{Guid.NewGuid():N}",
            $"valid_{Guid.NewGuid():N}@example.com",
            "SecurePass123!",
            "Valid",
            "User"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(request.Username);
        content.Should().Contain(request.Email.ToLowerInvariant()); // Email is normalized to lowercase
    }

    #endregion

    #region Validation Flow Ordering Tests

    [Fact]
    public async Task CreateUser_ShouldValidateInCorrectOrder()
    {
        // This test demonstrates the validation flow:
        // 1. Web API validates shape (ModelState) -> 400 BadRequest
        // 2. Application validates orchestration (username/email exists) -> 409 Conflict
        // 3. Domain validates business rules (Value Objects) -> DomainException -> 400

        // Test 1: Web API catches invalid format first
        var invalidFormatRequest = new CreateUserRequest(
            "test@invalid!", // Invalid format
            "duplicate@example.com", // Even if this email exists
            "SecurePass123!",
            "Test"
        );

        var response1 = await Client.PostAsJsonAsync("/api/users", invalidFormatRequest);
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Web API validation stops here

        // Test 2: Web API passes, Application catches duplicate
        // (Already tested in previous tests)

        // Test 3: Web API and Application pass, Domain validates business rules
        var weakDomainPassword = new CreateUserRequest(
            $"domaintest_{Guid.NewGuid():N}",
            $"domaintest_{Guid.NewGuid():N}@example.com",
            "weakpass", // Passes Web API (8 chars) but fails Domain validation
            "Domain",
            "Test"
        );

        var response3 = await Client.PostAsJsonAsync("/api/users", weakDomainPassword);
        // RFC 4918: 422 Unprocessable Entity - semantically incorrect
        response3.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion
}