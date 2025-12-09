namespace FAM.Infrastructure.Services.Email;

/// <summary>
/// Email provider configuration options
/// </summary>
public sealed class EmailProviderOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// Active provider: "Brevo", "Smtp", "SendGrid", etc.
    /// </summary>
    public string Provider { get; set; } = "Smtp";

    /// <summary>
    /// Default sender email
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Default sender name
    /// </summary>
    public string FromName { get; set; } = "FAM System";

    /// <summary>
    /// Brevo (Sendinblue) specific settings
    /// </summary>
    public BrevoOptions Brevo { get; set; } = new();

    /// <summary>
    /// SMTP specific settings
    /// </summary>
    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>
    /// SendGrid specific settings (for future use)
    /// </summary>
    public SendGridOptions SendGrid { get; set; } = new();
}

/// <summary>
/// Brevo (Sendinblue) provider options
/// </summary>
public sealed class BrevoOptions
{
    /// <summary>
    /// Brevo API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API Base URL (default: https://api.brevo.com/v3)
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.brevo.com/v3";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// SMTP provider options
/// </summary>
public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

/// <summary>
/// SendGrid provider options (for future implementation)
/// </summary>
public sealed class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
}