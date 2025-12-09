using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FAM.Application.Common.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FAM.Infrastructure.Services.Email.Providers;

/// <summary>
/// Brevo (formerly Sendinblue) email provider implementation
/// API Documentation: https://developers.brevo.com/reference/sendtransacemail
/// </summary>
public sealed class BrevoEmailProvider : IEmailProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrevoEmailProvider> _logger;
    private readonly BrevoOptions _options;
    private readonly string _defaultFromEmail;
    private readonly string _defaultFromName;

    public string ProviderName => "Brevo";

    public BrevoEmailProvider(
        HttpClient httpClient,
        IOptions<EmailProviderOptions> options,
        ILogger<BrevoEmailProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value.Brevo;
        _defaultFromEmail = options.Value.FromEmail;
        _defaultFromName = options.Value.FromName;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Brevo API key is not configured");
            return false;
        }

        try
        {
            // Check account info to verify API key
            var response = await _httpClient.GetAsync("/account", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Brevo availability");
            return false;
        }
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new BrevoSendEmailRequest
            {
                Sender = new BrevoEmailAddress
                {
                    Email = message.FromEmail ?? _defaultFromEmail,
                    Name = message.FromName ?? _defaultFromName
                },
                To = new[]
                {
                    new BrevoEmailAddress { Email = message.To }
                },
                Subject = message.Subject,
                HtmlContent = message.HtmlBody,
                TextContent = message.PlainTextBody
            };

            _logger.LogDebug("Sending email via Brevo to {To}", message.To);

            var response = await _httpClient.PostAsJsonAsync(
                "/smtp/email",
                request,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull },
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<BrevoSendEmailResponse>(responseContent);

                _logger.LogInformation(
                    "Email sent successfully via Brevo. MessageId: {MessageId}, To: {To}",
                    result?.MessageId, message.To);

                return EmailSendResult.Succeeded(result?.MessageId, responseContent);
            }

            _logger.LogError(
                "Failed to send email via Brevo. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, responseContent);

            return EmailSendResult.Failed($"Brevo API error: {response.StatusCode}", responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending email via Brevo to {To}", message.To);
            return EmailSendResult.Failed(ex.Message);
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

#region Brevo API Models

internal sealed class BrevoSendEmailRequest
{
    [JsonPropertyName("sender")] public BrevoEmailAddress Sender { get; set; } = null!;

    [JsonPropertyName("to")] public BrevoEmailAddress[] To { get; set; } = Array.Empty<BrevoEmailAddress>();

    [JsonPropertyName("subject")] public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("htmlContent")] public string? HtmlContent { get; set; }

    [JsonPropertyName("textContent")] public string? TextContent { get; set; }

    [JsonPropertyName("replyTo")] public BrevoEmailAddress? ReplyTo { get; set; }

    [JsonPropertyName("tags")] public string[]? Tags { get; set; }
}

internal sealed class BrevoEmailAddress
{
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string? Name { get; set; }
}

internal sealed class BrevoSendEmailResponse
{
    [JsonPropertyName("messageId")] public string? MessageId { get; set; }
}

#endregion