using System.Security.Cryptography;
using System.Text;

using FAM.Application.Abstractions;
using FAM.Domain.Abstractions;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// Token blacklist service using configurable cache provider (Redis/In-Memory)
/// Stores invalidated tokens with TTL matching their natural expiration
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ICacheProvider _cache;
    private readonly ILogger<TokenBlacklistService> _logger;
    private const string TokenBlacklistPrefix = "fam:token_blacklist:";
    private const string UserBlacklistPrefix = "fam:user_blacklist:";

    public TokenBlacklistService(ICacheProvider cache, ILogger<TokenBlacklistService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task BlacklistTokenAsync(string token, DateTime expiryTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Hash the token to avoid storing full JWT in cache
            string tokenHash = HashToken(token);
            string cacheKey = $"{TokenBlacklistPrefix}{tokenHash}";

            // Calculate TTL - how long until token naturally expires
            TimeSpan ttl = expiryTime - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                _logger.LogWarning("Attempted to blacklist an already expired token");
                return; // Token already expired, no need to blacklist
            }

            // Store a simple marker - we just need to know it exists
            await _cache.SetAsync(cacheKey, "blacklisted", ttl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting token");
            throw;
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            string tokenHash = HashToken(token);
            string cacheKey = $"{TokenBlacklistPrefix}{tokenHash}";

            string? value = await _cache.GetAsync(cacheKey, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token blacklist");
            // Fail open - don't block valid tokens if cache is down
            return false;
        }
    }

    public async Task BlacklistUserTokensAsync(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{UserBlacklistPrefix}{userId}";

            // Store for 24 hours - longer than typical access token lifetime
            // This will invalidate ALL tokens for this user
            TimeSpan expiration = TimeSpan.FromHours(24);
            await _cache.SetAsync(cacheKey, DateTime.UtcNow.ToString("O"), expiration, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting user tokens for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> AreUserTokensBlacklistedAsync(long userId, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{UserBlacklistPrefix}{userId}";
            string? value = await _cache.GetAsync(cacheKey, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user blacklist for user {UserId}", userId);
            // Fail open - don't block valid tokens if cache is down
            return false;
        }
    }

    public async Task BlacklistTokenByJtiAsync(string jti, DateTime expiryTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{TokenBlacklistPrefix}jti:{jti}";

            // Calculate TTL - how long until token naturally expires
            TimeSpan ttl = expiryTime - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                _logger.LogWarning("Attempted to blacklist token by JTI with already expired time: {JTI}", jti);
                return; // Token already expired, no need to blacklist
            }

            // Store a simple marker - we just need to know it exists
            await _cache.SetAsync(cacheKey, "blacklisted", ttl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blacklisting token by JTI: {JTI}", jti);
            throw;
        }
    }

    public async Task<bool> IsTokenBlacklistedByJtiAsync(string jti, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{TokenBlacklistPrefix}jti:{jti}";
            string? value = await _cache.GetAsync(cacheKey, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token blacklist by JTI: {JTI}", jti);
            // Fail open - don't block valid tokens if cache is down
            return false;
        }
    }

    /// <summary>
    /// Hash token using SHA256 to avoid storing full JWT
    /// </summary>
    private static string HashToken(string token)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
