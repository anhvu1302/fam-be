using System.Net;
using System.Net.Http.Json;
using FAM.WebApi.Models.Users;
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
        var request = new CreateUserRequestModel
        {
            Username = "testuser",
            Email = "invalid-email-format", // Invalid email format
            Password = "SecurePass123!",
            FullName = "Test User"
        };

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
        var request = new CreateUserRequestModel
        {
            Username = "", // Empty username
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

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
        var request = new CreateUserRequestModel
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "short", // Too short
            FullName = "Test User"
        };

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
        var request = new CreateUserRequestModel
        {
            Username = "invalid@username!", // Contains invalid characters
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username"); // ModelState error
    }

    [Fact]
    public async Task CreateUser_WithTooLongFullName_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateUserRequestModel
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = new string('a', 201) // Exceeds 200 character limit
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FullName"); // ModelState error
    }

    #endregion

    #region Application Layer Validation (Orchestration) - 409 Conflict

    [Fact]
    public async Task CreateUser_WithDuplicateUsername_ShouldReturn409Conflict()
    {
        // Arrange - Create first user
        var firstRequest = new CreateUserRequestModel
        {
            Username = "duplicateuser",
            Email = "first@example.com",
            Password = "SecurePass123!",
            FullName = "First User"
        };

        await Client.PostAsJsonAsync("/api/users", firstRequest);

        // Arrange - Try to create second user with same username
        var secondRequest = new CreateUserRequestModel
        {
            Username = "duplicateuser", // Same username
            Email = "second@example.com", // Different email
            Password = "SecurePass123!",
            FullName = "Second User"
        };

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
        var firstRequest = new CreateUserRequestModel
        {
            Username = "firstuser",
            Email = "duplicate@example.com",
            Password = "SecurePass123!",
            FullName = "First User"
        };

        await Client.PostAsJsonAsync("/api/users", firstRequest);

        // Arrange - Try to create second user with same email
        var secondRequest = new CreateUserRequestModel
        {
            Username = "seconduser", // Different username
            Email = "duplicate@example.com", // Same email
            Password = "SecurePass123!",
            FullName = "Second User"
        };

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
        var request = new UpdateUserRequestModel
        {
            Username = "newusername",
            Email = "new@example.com",
            Password = "NewPass123!",
            FullName = "New Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{nonExistentUserId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Domain Layer Validation (Business Rules) - DomainException mapped to 400

    [Fact]
    public async Task CreateUser_WithWeakPassword_ShouldReturnWebAPIValidationError()
    {
        // Arrange
        var request = new CreateUserRequestModel
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "weakpass", // Passes Web API min length (8 chars) but ALSO passes DataAnnotations
            // NOTE: Web API only validates min length, not password strength
            // Domain would validate strength, but this password passes Web API layer
            FullName = "Test User"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        // This password actually passes Web API validation (8 chars minimum)
        // It will fail at Domain layer (Password.Create) due to missing uppercase/number/special char
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Domain validation returns 400
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password"); // Domain validation error
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateUserRequestModel
        {
            Username = $"validuser_{Guid.NewGuid():N}",
            Email = $"valid_{Guid.NewGuid():N}@example.com",
            Password = "SecurePass123!",
            FullName = "Valid User"
        };

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
        var invalidFormatRequest = new CreateUserRequestModel
        {
            Username = "test@invalid!", // Invalid format
            Email = "duplicate@example.com", // Even if this email exists
            Password = "SecurePass123!",
            FullName = "Test"
        };

        var response1 = await Client.PostAsJsonAsync("/api/users", invalidFormatRequest);
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Web API validation stops here

        // Test 2: Web API passes, Application catches duplicate
        // (Already tested in previous tests)

        // Test 3: Web API and Application pass, Domain validates business rules
        var weakDomainPassword = new CreateUserRequestModel
        {
            Username = $"domaintest_{Guid.NewGuid():N}",
            Email = $"domaintest_{Guid.NewGuid():N}@example.com",
            Password = "weakpass", // Passes Web API (8 chars) but fails Domain validation
            FullName = "Domain Test"
        };

        var response3 = await Client.PostAsJsonAsync("/api/users", weakDomainPassword);
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Domain exception returns 400
    }

    #endregion
}
