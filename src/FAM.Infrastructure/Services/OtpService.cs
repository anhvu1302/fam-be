using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using FAM.Application.Abstractions;
using FAM.Application.Common.Services;

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
        var otp = GenerateSixDigitOtp();

        // SECURITY: Sử dụng hash của sessionToken để làm key
        // Điều này đảm bảo OTP chỉ dùng được với đúng session
        var cacheKey = GenerateSecureCacheKey(userId, sessionToken);

        var otpData = new OtpData
        {
            Code = otp,
            UserId = userId,
            SessionTokenHash = HashSessionToken(sessionToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        var serialized = JsonSerializer.Serialize(otpData);
        var expiration = TimeSpan.FromMinutes(expirationMinutes);

        await _cache.SetAsync(cacheKey, serialized, expiration, cancellationToken);

        // Reset attempt counter
        var attemptKey = GenerateAttemptKey(userId, sessionToken);
        await _cache.DeleteAsync(attemptKey, cancellationToken);

        return otp;
    }

    public async Task<bool> VerifyOtpAsync(long userId, string sessionToken, string otpCode,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateSecureCacheKey(userId, sessionToken);
        var attemptKey = GenerateAttemptKey(userId, sessionToken);

        // Check attempts (rate limiting)
        var attempts = await GetAttemptsAsync(attemptKey, cancellationToken);
        if (attempts >= MaxAttempts)
        {
            _logger.LogWarning("Max OTP attempts reached for user {UserId}", userId);
            return false;
        }

        var cached = await _cache.GetAsync(cacheKey, cancellationToken);

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
            var currentSessionHash = HashSessionToken(sessionToken);
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

            var isValid = otpData.Code == otpCode;

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
        var cacheKey = GenerateSecureCacheKey(userId, sessionToken);
        var attemptKey = GenerateAttemptKey(userId, sessionToken);

        await _cache.DeleteAsync(cacheKey, cancellationToken);
        await _cache.DeleteAsync(attemptKey, cancellationToken);
    }

    #region Private Helper Methods

    private static string GenerateSecureCacheKey(long userId, string sessionToken)
    {
        // Hash session token để tạo key unique cho mỗi session
        var hash = HashSessionToken(sessionToken);
        return $"{OtpKeyPrefix}{userId}:{hash[..16]}"; // Lấy 16 ký tự đầu của hash
    }

    private static string GenerateAttemptKey(long userId, string sessionToken)
    {
        var hash = HashSessionToken(sessionToken);
        return $"{AttemptKeyPrefix}{userId}:{hash[..16]}";
    }

    private static string HashSessionToken(string sessionToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(sessionToken);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<int> GetAttemptsAsync(string attemptKey, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync(attemptKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(cached))
            return 0;

        if (int.TryParse(cached, out var attempts))
            return attempts;

        return 0;
    }

    private async Task IncrementAttemptsAsync(string attemptKey, CancellationToken cancellationToken)
    {
        var current = await GetAttemptsAsync(attemptKey, cancellationToken);
        var newCount = current + 1;

        var expiration = TimeSpan.FromMinutes(15); // Reset after 15 minutes
        await _cache.SetAsync(attemptKey, newCount.ToString(), expiration, cancellationToken);

        if (newCount >= MaxAttempts) _logger.LogWarning("User reached max OTP attempts: {Attempts}", newCount);
    }

    private static string GenerateSixDigitOtp()
    {
        // Generate cryptographically secure 6-digit OTP
        var randomNumber = RandomNumberGenerator.GetInt32(100000, 999999);
        return randomNumber.ToString();
    }

    #endregion

    private class OtpData
    {
        public string Code { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string SessionTokenHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
