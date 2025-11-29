using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FAM.Contracts.Storage;
using Xunit;

namespace FAM.IntegrationTests.Storage;

public class StorageControllerTests : IClassFixture<FamWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly FamWebApplicationFactory _factory;
    private string? _accessToken;

    public StorageControllerTests(FamWebApplicationFactory factory)
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

    private async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken))
            return _accessToken;

        var loginRequest = new
        {
            identity = "admin",
            password = "Admin@123"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with {response.StatusCode}: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        _accessToken = loginResponse.GetProperty("accessToken").GetString();

        return _accessToken!;
    }

    [Fact]
    public async Task UploadFile_ValidImage_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(new byte[1024]); // 1 KB test file
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "file", "test-image.jpg");

        // Act
        var response = await _client.PostAsync("/api/storage/upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UploadFileResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result.FilePath);
        Assert.NotEmpty(result.Url);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(1024, result.FileSize);
    }

    [Fact]
    public async Task UploadFile_UnsupportedExtension_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[100]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "malware.exe");

        // Act
        var response = await _client.PostAsync("/api/storage/upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("not supported", responseContent, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tests application-level file size validation via FileValidator.
    /// MaxImageSizeMb = 10 (from appsettings.json), so 11MB file should be rejected.
    /// </summary>
    [Fact]
    public async Task UploadFile_FileTooLarge_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        // 11 MB file exceeds MaxImageSizeMb = 10 (from appsettings.json)
        var largeImageContent = new ByteArrayContent(new byte[11 * 1024 * 1024]);
        largeImageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(largeImageContent, "file", "large-image.jpg");

        // Act
        var response = await _client.PostAsync("/api/storage/upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("too large", responseContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadFile_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[100]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/storage/upload", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InitiateMultipartUpload_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new InitiateMultipartUploadRequest
        {
            FileName = "large-video.mp4",
            ContentType = "video/mp4",
            TotalSize = 10 * 1024 * 1024 // 10 MB
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/storage/multipart/initiate", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<InitiateMultipartUploadResponse>(responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.NotNull(result);
        Assert.NotEmpty(result.UploadId);
        Assert.NotEmpty(result.FilePath);
        Assert.True(result.ChunkSize > 0);
    }

    [Fact]
    public async Task InitiateMultipartUpload_UnsupportedFileType_ReturnsBadRequest()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new InitiateMultipartUploadRequest
        {
            FileName = "malware.exe",
            ContentType = "application/octet-stream",
            TotalSize = 10 * 1024 * 1024
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/storage/multipart/initiate", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPresignedUrl_ValidFilePath_ReturnsUrl()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First upload a file
        var uploadContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[1024]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(fileContent, "file", "test.jpg");

        var uploadResponse = await _client.PostAsync("/api/storage/upload", uploadContent);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResult = JsonSerializer.Deserialize<UploadFileResponse>(
            await uploadResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Get presigned URL
        var request = new GetPresignedUrlRequest
        {
            FilePath = uploadResult!.FilePath,
            ExpiryInSeconds = 300 // 5 minutes
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/storage/presigned-url", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetPresignedUrlResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result.Url);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GetPresignedUrl_NonExistentFile_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new GetPresignedUrlRequest
        {
            FilePath = "images/non-existent-file.jpg",
            ExpiryInSeconds = 3600
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/storage/presigned-url", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFile_ExistingFile_ReturnsNoContent()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First upload a file
        var uploadContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[1024]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(fileContent, "file", "test-delete.jpg");

        var uploadResponse = await _client.PostAsync("/api/storage/upload", uploadContent);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResult = JsonSerializer.Deserialize<UploadFileResponse>(
            await uploadResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        var response = await _client.DeleteAsync($"/api/storage/{uploadResult!.FilePath}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFile_NonExistentFile_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.DeleteAsync("/api/storage/images/non-existent.jpg");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFileInfo_ExistingFile_ReturnsInfo()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // First upload a file
        var uploadContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[2048]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        uploadContent.Add(fileContent, "file", "test-info.jpg");

        var uploadResponse = await _client.PostAsync("/api/storage/upload", uploadContent);
        uploadResponse.EnsureSuccessStatusCode();

        var uploadResult = JsonSerializer.Deserialize<UploadFileResponse>(
            await uploadResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Act
        var response = await _client.GetAsync($"/api/storage/info/{uploadResult!.FilePath}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var fileInfo = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // JsonElement is value type, no need for null check
        Assert.Equal(uploadResult.FilePath, fileInfo.GetProperty("filePath").GetString());
        Assert.Equal(2048, fileInfo.GetProperty("size").GetInt64());
    }

    [Fact]
    public async Task GetFileInfo_NonExistentFile_ReturnsNotFound()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/storage/info/images/non-existent.jpg");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}