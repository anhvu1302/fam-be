using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using FAM.Application.Abstractions;
using FAM.Application.Common.Services;
using FAM.Infrastructure.Services.Dtos;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// OTP service using configurable cache provider (Redis/In-Memory)
/// SECURED: OTP được bind với session token để tránh replay attacks
/// </summary>
public class OtpService : IOtpService
{
    private readonly ICacheProvider _cache;
    private readonly ILogger<OtpService> _logger;
    private const string OtpKeyPrefix = "fam:otp:";
    private const string AttemptKeyPrefix = "fam:otp_attempts:";
    private const int MaxAttempts = 5;

    public OtpService(ICacheProvider cache, ILogger<OtpService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateOtpAsync(long userId, string sessionToken, int expirationMinutes = 10,
        CancellationToken cancellationToken = default)
    {
        // Generate 6-digit OTP
        string otp = GenerateSixDigitOtp();

        // SECURITY: Sử dụng hash của sessionToken để làm key
        // Điều này đảm bảo OTP chỉ dùng được với đúng session
        string cacheKey = GenerateSecureCacheKey(userId, sessionToken);

        OtpData otpData = new()
        {
            Code = otp,
            UserId = userId,
            SessionTokenHash = HashSessionToken(sessionToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        string serialized = JsonSerializer.Serialize(otpData);
        TimeSpan expiration = TimeSpan.FromMinutes(expirationMinutes);

        await _cache.SetAsync(cacheKey, serialized, expiration, cancellationToken);

        // Reset attempt counter
        string attemptKey = GenerateAttemptKey(userId, sessionToken);
        await _cache.DeleteAsync(attemptKey, cancellationToken);

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(long userId, string sessionToken, string otpCode,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = GenerateSecureCacheKey(userId, sessionToken);
        string attemptKey = GenerateAttemptKey(userId, sessionToken);

        // Check attempts (rate limiting)
        int attempts = await GetAttemptsAsync(attemptKey, cancellationToken);
        if (attempts >= MaxAttempts)
        {
            _logger.LogWarning("Max OTP attempts reached for user {UserId}", userId);
            return false;
        }

        string? cached = await _cache.GetAsync(cacheKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(cached))
        {
            _logger.LogWarning("No OTP found for user {UserId} with this session", userId);
            await IncrementAttemptsAsync(attemptKey, cancellationToken);
            return false;
        }

        try
        {
            OtpData? otpData = JsonSerializer.Deserialize<OtpData>(cached);
            if (otpData == null)
            {
                _logger.LogWarning("Failed to deserialize OTP data for user {UserId}", userId);
                await IncrementAttemptsAsync(attemptKey, cancellationToken);
                return false;
            }

            // SECURITY: Verify session token hash
            string currentSessionHash = HashSessionToken(sessionToken);
            if (otpData.SessionTokenHash != currentSessionHash)
            {
                _logger.LogWarning("Session token mismatch for user {UserId} - possible attack", userId);
                await IncrementAttemptsAsync(attemptKey, cancellationToken);
                return false;
            }

            if (otpData.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("OTP expired for user {UserId}", userId);
                await _cache.DeleteAsync(cacheKey, cancellationToken);
                await IncrementAttemptsAsync(attemptKey, cancellationToken);
                return false;
            }

            bool isValid = otpData.Code == otpCode;

            if (isValid)
            {
                // Clean up on success
                await _cache.DeleteAsync(cacheKey, cancellationToken);
                await _cache.DeleteAsync(attemptKey, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Invalid OTP provided for user {UserId}", userId);
                await IncrementAttemptsAsync(attemptKey, cancellationToken);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for user {UserId}", userId);
            await IncrementAttemptsAsync(attemptKey, cancellationToken);
            return false;
        }
    }

    public async Task RemoveOtpAsync(long userId, string sessionToken, CancellationToken cancellationToken = default)
    {
        string cacheKey = GenerateSecureCacheKey(userId, sessionToken);
        string attemptKey = GenerateAttemptKey(userId, sessionToken);

        await _cache.DeleteAsync(cacheKey, cancellationToken);
        await _cache.DeleteAsync(attemptKey, cancellationToken);
    }

    #region Private Helper Methods

    private static string GenerateSecureCacheKey(long userId, string sessionToken)
    {
        // Hash session token để tạo key unique cho mỗi session
        string hash = HashSessionToken(sessionToken);
        return $"{OtpKeyPrefix}{userId}:{hash[..16]}"; // Lấy 16 ký tự đầu của hash
    }

    private static string GenerateAttemptKey(long userId, string sessionToken)
    {
        string hash = HashSessionToken(sessionToken);
        return $"{AttemptKeyPrefix}{userId}:{hash[..16]}";
    }

    private static string HashSessionToken(string sessionToken)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(sessionToken);
        byte[] hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<int> GetAttemptsAsync(string attemptKey, CancellationToken cancellationToken)
    {
        string? cached = await _cache.GetAsync(attemptKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(cached))
        {
            return 0;
        }

        if (int.TryParse(cached, out int attempts))
        {
            return attempts;
        }

        return 0;
    }

    private async Task IncrementAttemptsAsync(string attemptKey, CancellationToken cancellationToken)
    {
        int current = await GetAttemptsAsync(attemptKey, cancellationToken);
        int newCount = current + 1;

        TimeSpan expiration = TimeSpan.FromMinutes(15); // Reset after 15 minutes
        await _cache.SetAsync(attemptKey, newCount.ToString(), expiration, cancellationToken);

        if (newCount >= MaxAttempts)
        {
            _logger.LogWarning("User reached max OTP attempts: {Attempts}", newCount);
        }
    }

    private static string GenerateSixDigitOtp()
    {
        // Generate cryptographically secure 6-digit OTP
        int randomNumber = RandomNumberGenerator.GetInt32(100000, 999999);
        return randomNumber.ToString();
    }

    #endregion
}
