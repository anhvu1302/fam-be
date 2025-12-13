using System.Net;
using System.Net.Mail;
using System.Text;

using FAM.Application.Common.Services;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;

using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// Email service implementation using SMTP with database-driven email templates
/// SMTP settings are loaded from environment variables via AppConfiguration
/// Email templates are loaded from database (EmailTemplate entity)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _enableSsl;

    public EmailService(
        ILogger<EmailService> logger,
        IUnitOfWork unitOfWork,
        string smtpHost,
        int smtpPort,
        string smtpUsername,
        string smtpPassword,
        bool smtpEnableSsl,
        string emailFrom,
        string emailFromName)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _smtpUser = smtpUsername;
        _smtpPassword = smtpPassword;
        _fromEmail = emailFrom;
        _fromName = emailFromName;
        _enableSsl = smtpEnableSsl;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, string userName,
        CancellationToken cancellationToken = default)
    {

        // Get template from database
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByCodeAsync("OTP_EMAIL", cancellationToken);
        if (template == null)
        {
            _logger.LogError("Email template OTP_EMAIL not found");
            throw new InvalidOperationException("Email template OTP_EMAIL not found. Please run database seeder.");
        }

        // Replace placeholders
        var placeholders = new Dictionary<string, string>
        {
            { "userName", userName },
            { "otpCode", otpCode },
            { "expiryMinutes", "10" },
            { "appName", "FAM System" },
            { "currentYear", DateTime.UtcNow.Year.ToString() }
        };

        var subject = ReplacePlaceholders(template.Subject, placeholders);
        var body = ReplacePlaceholders(template.HtmlBody, placeholders);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName, string resetUrl,
        int expiryMinutes, CancellationToken cancellationToken = default)
    {

        // Get template from database
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByCodeAsync("PASSWORD_RESET", cancellationToken);
        if (template == null)
        {
            _logger.LogError("Email template PASSWORD_RESET not found");
            throw new InvalidOperationException("Email template PASSWORD_RESET not found. Please run database seeder.");
        }

        // Build reset link
        var resetLink = $"{resetUrl}?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

        // Replace placeholders
        var placeholders = new Dictionary<string, string>
        {
            { "userName", userName },
            { "resetLink", resetLink },
            { "expiryMinutes", expiryMinutes.ToString() },
            { "expiryHours", "1" },
            { "appName", "FAM System" },
            { "currentYear", DateTime.UtcNow.Year.ToString() }
        };

        var subject = ReplacePlaceholders(template.Subject, placeholders);
        var body = ReplacePlaceholders(template.HtmlBody, placeholders);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordChangedEmailAsync(string toEmail, string userName,
        CancellationToken cancellationToken = default)
    {

        // Get template from database
        EmailTemplate? template =
            await _unitOfWork.EmailTemplates.GetByCodeAsync("PASSWORD_CHANGED", cancellationToken);
        if (template == null)
        {
            _logger.LogError("Email template PASSWORD_CHANGED not found");
            throw new InvalidOperationException(
                "Email template PASSWORD_CHANGED not found. Please run database seeder.");
        }

        // Replace placeholders
        var placeholders = new Dictionary<string, string>
        {
            { "userName", userName },
            { "changeTime", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            { "appName", "FAM System" },
            { "currentYear", DateTime.UtcNow.Year.ToString() }
        };

        var subject = ReplacePlaceholders(template.Subject, placeholders);
        var body = ReplacePlaceholders(template.HtmlBody, placeholders);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if SMTP is configured
            if (string.IsNullOrWhiteSpace(_smtpUser) || string.IsNullOrWhiteSpace(_smtpPassword))
            {
                _logger.LogWarning("SMTP not configured. Email would be sent to {Email} with subject: {Subject}",
                    toEmail, subject);
                _logger.LogDebug("Email body: {Body}", htmlBody);
                return; // Skip actual sending in development
            }

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    /// <summary>
    /// Replace placeholders in template string
    /// Supports both {{placeholder}} and {placeholder} formats
    /// </summary>
    private static string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
    {
        var result = template;

        foreach (var (key, value) in placeholders)
        {
            // Replace {{key}} format
            result = result.Replace($"{{{{{key}}}}}", value);
            // Replace {key} format
            result = result.Replace($"{{{key}}}", value);
        }

        return result;
    }
}
