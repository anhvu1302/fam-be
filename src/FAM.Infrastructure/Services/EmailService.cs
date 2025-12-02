using System.Net;
using System.Net.Mail;
using System.Text;
using FAM.Application.Common.Services;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Services;

/// <summary>
/// Email service implementation using SMTP
/// SMTP settings are loaded from environment variables via AppConfiguration
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly bool _enableSsl;

    public EmailService(ILogger<EmailService> logger, string smtpHost, int smtpPort,
        string smtpUsername, string smtpPassword, bool smtpEnableSsl,
        string emailFrom, string emailFromName)
    {
        _logger = logger;
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
        _logger.LogInformation("Sending OTP email to {Email}", toEmail);

        var subject = "Your Verification Code - FAM";
        var body = GenerateOtpEmailTemplate(userName, otpCode);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName, string resetUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending password reset email to {Email}", toEmail);

        var subject = "Reset Your Password - FAM";
        var resetLink = $"{resetUrl}?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";
        var body = GeneratePasswordResetEmailTemplate(userName, resetLink);

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendPasswordChangedEmailAsync(string toEmail, string userName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending password changed confirmation to {Email}", toEmail);

        var subject = "Password Changed Successfully - FAM";
        var body = GeneratePasswordChangedEmailTemplate(userName);

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

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    private string GenerateOtpEmailTemplate(string userName, string otpCode)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Verification Code</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4; padding: 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">🔐 Verification Code</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""color: #333333; font-size: 16px; line-height: 24px; margin: 0 0 20px 0;"">Hi <strong>{userName}</strong>,</p>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 0 0 30px 0;"">
                                You've requested a verification code to complete your login. Use the code below:
                            </p>
                            
                            <!-- OTP Code Box -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                                <tr>
                                    <td align=""center"" style=""background-color: #f8f9fa; padding: 30px; border-radius: 8px; border: 2px dashed #667eea;"">
                                        <div style=""font-size: 36px; font-weight: bold; color: #667eea; letter-spacing: 8px; font-family: 'Courier New', monospace;"">
                                            {otpCode}
                                        </div>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 20px 0;"">
                                This code will <strong>expire in 10 minutes</strong>. Do not share this code with anyone.
                            </p>
                            
                            <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                                <p style=""color: #856404; font-size: 13px; margin: 0; line-height: 20px;"">
                                    ⚠️ <strong>Security Notice:</strong> If you didn't request this code, please ignore this email and ensure your account is secure.
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0 0 10px 0;"">
                                This is an automated message from <strong>FAM System</strong>
                            </p>
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                © 2024 Fixed Asset Management. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
        ";
    }

    private string GeneratePasswordResetEmailTemplate(string userName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4; padding: 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">🔑 Reset Your Password</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""color: #333333; font-size: 16px; line-height: 24px; margin: 0 0 20px 0;"">Hi <strong>{userName}</strong>,</p>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 0 0 20px 0;"">
                                We received a request to reset your password for your Fixed Asset Management account.
                            </p>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 0 0 30px 0;"">
                                Click the button below to create a new password:
                            </p>
                            
                            <!-- Reset Button -->
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{resetLink}"" style=""display: inline-block; padding: 16px 40px; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px; box-shadow: 0 4px 6px rgba(245,87,108,0.3);"">
                                            Reset Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""color: #666666; font-size: 13px; line-height: 20px; margin: 20px 0;"">
                                Or copy and paste this link into your browser:
                            </p>
                            
                            <div style=""background-color: #f8f9fa; padding: 15px; border-radius: 4px; word-break: break-all; margin: 10px 0 20px 0;"">
                                <p style=""color: #667eea; font-size: 12px; margin: 0; font-family: monospace;"">{resetLink}</p>
                            </div>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 20px 0;"">
                                This link will <strong>expire in 1 hour</strong> for security reasons.
                            </p>
                            
                            <div style=""background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                                <p style=""color: #721c24; font-size: 13px; margin: 0; line-height: 20px;"">
                                    🚨 <strong>Important:</strong> If you didn't request a password reset, please ignore this email or contact support if you're concerned about your account security.
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0 0 10px 0;"">
                                This is an automated message from <strong>FAM System</strong>
                            </p>
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                © 2024 Fixed Asset Management. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
        ";
    }

    private string GeneratePasswordChangedEmailTemplate(string userName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Changed</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4; padding: 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 40px 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">✅ Password Changed Successfully</h1>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""color: #333333; font-size: 16px; line-height: 24px; margin: 0 0 20px 0;"">Hi <strong>{userName}</strong>,</p>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 0 0 20px 0;"">
                                This is a confirmation that your password has been changed successfully.
                            </p>
                            
                            <div style=""background-color: #d4edda; border-left: 4px solid #28a745; padding: 20px; margin: 20px 0; border-radius: 4px;"">
                                <p style=""color: #155724; font-size: 14px; margin: 0 0 10px 0; font-weight: 600;"">
                                    ✓ Your password was successfully updated
                                </p>
                                <p style=""color: #155724; font-size: 13px; margin: 0;"">
                                    Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                                </p>
                            </div>
                            
                            <p style=""color: #666666; font-size: 14px; line-height: 22px; margin: 20px 0;"">
                                You can now use your new password to log in to your Fixed Asset Management account.
                            </p>
                            
                            <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px;"">
                                <p style=""color: #856404; font-size: 13px; margin: 0; line-height: 20px;"">
                                    ⚠️ <strong>Did not make this change?</strong> If you didn't change your password, please contact our support team immediately and secure your account.
                                </p>
                            </div>
                            
                            <div style=""margin-top: 30px; padding-top: 20px; border-top: 1px solid #e9ecef;"">
                                <p style=""color: #666666; font-size: 13px; line-height: 20px; margin: 0;"">
                                    <strong>Security Tips:</strong>
                                </p>
                                <ul style=""color: #666666; font-size: 13px; line-height: 24px; margin: 10px 0 0 20px;"">
                                    <li>Use a unique password for each account</li>
                                    <li>Enable two-factor authentication for added security</li>
                                    <li>Never share your password with anyone</li>
                                </ul>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0 0 10px 0;"">
                                This is an automated message from <strong>FAM System</strong>
                            </p>
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                © 2024 Fixed Asset Management. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
        ";
    }
}