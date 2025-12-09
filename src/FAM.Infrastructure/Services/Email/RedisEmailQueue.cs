using System.Text.Json;
using FAM.Application.Common.Email;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FAM.Infrastructure.Services.Email;

/// <summary>
/// Redis-backed email queue for persistence across app restarts
/// Uses Redis Lists for FIFO queue behavior with atomic operations
/// </summary>
public sealed class RedisEmailQueue : IEmailQueue
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEmailQueue> _logger;

    private const string QueueKey = "fam:email:queue";
    private const string ProcessingKey = "fam:email:processing";
    private const string FailedKey = "fam:email:failed";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisEmailQueue(IConnectionMultiplexer redis, ILogger<RedisEmailQueue> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public int Count
    {
        get
        {
            try
            {
                var db = _redis.GetDatabase();
                return (int)db.ListLength(QueueKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get queue count from Redis");
                return 0;
            }
        }
    }

    public async ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(message, JsonOptions);

            // RPUSH for FIFO queue (add to right, take from left)
            await db.ListRightPushAsync(QueueKey, json);

            _logger.LogDebug(
                "Email queued to Redis: {EmailId} to {To}, Priority: {Priority}, Queue size: {Count}",
                message.Id, message.To, message.Priority, Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue email {EmailId} to Redis", message.Id);
            throw;
        }
    }

    public async ValueTask<EmailMessage?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();

            // LPOP to get from left (FIFO)
            var json = await db.ListLeftPopAsync(QueueKey);

            if (json.IsNullOrEmpty) return null;

            var message = JsonSerializer.Deserialize<EmailMessage>(json!, JsonOptions);

            if (message != null)
                _logger.LogDebug(
                    "Email dequeued from Redis: {EmailId} to {To}, Queue size: {Count}",
                    message.Id, message.To, Count);

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dequeue email from Redis");
            return null;
        }
    }

    /// <summary>
    /// Move failed email to failed queue for manual review
    /// </summary>
    public async Task MoveToFailedAsync(EmailMessage message, string errorMessage)
    {
        try
        {
            var db = _redis.GetDatabase();
            var failedMessage = new
            {
                Email = message,
                Error = errorMessage,
                FailedAt = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(failedMessage, JsonOptions);

            await db.ListRightPushAsync(FailedKey, json);

            _logger.LogWarning(
                "Email {EmailId} moved to failed queue: {Error}",
                message.Id, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move email {EmailId} to failed queue", message.Id);
        }
    }

    /// <summary>
    /// Get count of failed emails
    /// </summary>
    public int FailedCount
    {
        get
        {
            try
            {
                var db = _redis.GetDatabase();
                return (int)db.ListLength(FailedKey);
            }
            catch
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Retry all failed emails by moving them back to the main queue
    /// </summary>
    public async Task<int> RetryFailedEmailsAsync()
    {
        var db = _redis.GetDatabase();
        var retried = 0;

        while (true)
        {
            var json = await db.ListLeftPopAsync(FailedKey);
            if (json.IsNullOrEmpty) break;

            // Extract just the email part and re-queue
            try
            {
                using var doc = JsonDocument.Parse(json.ToString());
                var emailJson = doc.RootElement.GetProperty("email").GetRawText();
                await db.ListRightPushAsync(QueueKey, emailJson);
                retried++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retry email from failed queue");
            }
        }

        _logger.LogInformation("Retried {Count} failed emails", retried);
        return retried;
    }
}