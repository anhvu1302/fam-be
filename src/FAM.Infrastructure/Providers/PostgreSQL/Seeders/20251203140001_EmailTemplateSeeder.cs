using FAM.Domain.EmailTemplates;
using FAM.Infrastructure.Common.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FAM.Infrastructure.Providers.PostgreSQL.Seeders;

/// <summary>
/// Seeds initial email templates for PostgreSQL
/// </summary>
public class EmailTemplateSeeder : BaseDataSeeder
{
    private readonly PostgreSqlDbContext _dbContext;

    public EmailTemplateSeeder(PostgreSqlDbContext dbContext, ILogger<EmailTemplateSeeder> logger)
        : base(logger)
    {
        _dbContext = dbContext;
    }

    public override string Name => "20251203140001_EmailTemplateSeeder";

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInfo("Starting to seed email templates...");

        // Check if templates already exist
        if (await _dbContext.EmailTemplates.AnyAsync(cancellationToken))
        {
            LogInfo("Email templates already exist, skipping seed");
            return;
        }

        EmailTemplate[] templates = new[]
        {
            // OTP Email Template
            EmailTemplate.Create(
                "OTP_EMAIL",
                "OTP Verification Code",
                "Your verification code - {{appName}}",
                @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>OTP Verification</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; font-size: 24px; margin: 0 0 20px 0;"">Hello {{userName}},</h1>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                You requested a verification code for {{appName}}. Please use the code below:
                            </p>
                            <div style=""background-color: #f8f9fa; border-radius: 8px; padding: 30px; text-align: center; margin: 30px 0;"">
                                <div style=""font-size: 36px; font-weight: bold; color: #007bff; letter-spacing: 8px;"">{{otpCode}}</div>
                            </div>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.5; margin: 0 0 10px 0;"">
                                This code will expire in <strong>{{expiryMinutes}} minutes</strong>.
                            </p>
                            <p style=""color: #999999; font-size: 13px; line-height: 1.5; margin: 30px 0 0 0;"">
                                If you didn't request this code, please ignore this email or contact support if you have concerns.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 30px; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.5; margin: 0; text-align: center;"">
                                © 2025 {{appName}}. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                EmailTemplateCategory.Authentication,
                "Email template for sending OTP verification codes",
                @"Hello {{userName}},

You requested a verification code for {{appName}}. Please use the code below:

{{otpCode}}

This code will expire in {{expiryMinutes}} minutes.

If you didn't request this code, please ignore this email.

© 2025 {{appName}}. All rights reserved.",
                "[\"userName\",\"otpCode\",\"expiryMinutes\",\"appName\"]",
                true
            ),

            // Password Reset Template
            EmailTemplate.Create(
                "PASSWORD_RESET",
                "Password Reset Request",
                "Reset your password - {{appName}}",
                @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; font-size: 24px; margin: 0 0 20px 0;"">Hello {{userName}},</h1>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                We received a request to reset your password for your {{appName}} account. Click the button below to reset it:
                            </p>
                            <table role=""presentation"" style=""margin: 30px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{{resetLink}}"" style=""background-color: #007bff; color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;"">
                                            Reset Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.5; margin: 0 0 10px 0;"">
                                This link will expire in <strong>{{expiryMinutes}} minutes</strong>.
                            </p>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.5; margin: 10px 0;"">
                                If the button doesn't work, copy and paste this link into your browser:
                            </p>
                            <p style=""color: #007bff; font-size: 13px; word-break: break-all; margin: 0 0 20px 0;"">
                                {{resetLink}}
                            </p>
                            <p style=""color: #999999; font-size: 13px; line-height: 1.5; margin: 30px 0 0 0;"">
                                If you didn't request a password reset, please ignore this email or contact support if you have concerns.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 30px; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.5; margin: 0; text-align: center;"">
                                © 2025 {{appName}}. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                EmailTemplateCategory.Authentication,
                "Email template for password reset requests",
                @"Hello {{userName}},

We received a request to reset your password for your {{appName}} account.

Please click the link below to reset your password:
{{resetLink}}

This link will expire in {{expiryMinutes}} minutes.

If you didn't request a password reset, please ignore this email.

© 2025 {{appName}}. All rights reserved.",
                "[\"userName\",\"resetLink\",\"expiryMinutes\",\"appName\"]",
                true
            ),

            // Password Changed Template
            EmailTemplate.Create(
                "PASSWORD_CHANGED",
                "Password Changed Confirmation",
                "Your password was changed - {{appName}}",
                @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Changed</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; font-size: 24px; margin: 0 0 20px 0;"">Hello {{userName}},</h1>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                Your password for {{appName}} has been successfully changed.
                            </p>
                            <div style=""background-color: #f8f9fa; border-left: 4px solid #28a745; padding: 20px; margin: 20px 0;"">
                                <p style=""color: #666666; font-size: 14px; margin: 0 0 10px 0;"">
                                    <strong>Change Time:</strong> {{changeTime}}
                                </p>
                                <p style=""color: #666666; font-size: 14px; margin: 0;"">
                                    <strong>IP Address:</strong> {{ipAddress}}
                                </p>
                            </div>
                            <p style=""color: #999999; font-size: 13px; line-height: 1.5; margin: 30px 0 0 0;"">
                                If you didn't make this change, please contact our support team immediately.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 30px; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.5; margin: 0; text-align: center;"">
                                © 2025 {{appName}}. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                EmailTemplateCategory.Authentication,
                "Email template for password change confirmation",
                @"Hello {{userName}},

Your password for {{appName}} has been successfully changed.

Change Time: {{changeTime}}
IP Address: {{ipAddress}}

If you didn't make this change, please contact our support team immediately.

© 2025 {{appName}}. All rights reserved.",
                "[\"userName\",\"changeTime\",\"ipAddress\",\"appName\"]",
                true
            ),

            // Welcome Email Template
            EmailTemplate.Create(
                "WELCOME_EMAIL",
                "Welcome New User",
                "Welcome to {{appName}}!",
                @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #333333; font-size: 28px; margin: 0 0 20px 0;"">Welcome to {{appName}}! 🎉</h1>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                Hello <strong>{{userName}}</strong>,
                            </p>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                Your account has been successfully created! We're excited to have you on board.
                            </p>
                            <div style=""background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin: 20px 0;"">
                                <p style=""color: #666666; font-size: 14px; margin: 0 0 10px 0;"">
                                    <strong>Your Email:</strong> {{email}}
                                </p>
                            </div>
                            <table role=""presentation"" style=""margin: 30px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{{loginUrl}}"" style=""background-color: #007bff; color: #ffffff; padding: 15px 40px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;"">
                                            Login Now
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.5; margin: 20px 0 0 0;"">
                                If you have any questions, feel free to contact our support team.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 30px; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.5; margin: 0; text-align: center;"">
                                © 2025 {{appName}}. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                EmailTemplateCategory.Notification,
                "Email template for welcoming new users",
                @"Welcome to {{appName}}!

Hello {{userName}},

Your account has been successfully created! We're excited to have you on board.

Your Email: {{email}}

Login at: {{loginUrl}}

If you have any questions, feel free to contact our support team.

© 2025 {{appName}}. All rights reserved.",
                "[\"userName\",\"email\",\"loginUrl\",\"appName\"]",
                true
            ),

            // Account Locked Template
            EmailTemplate.Create(
                "ACCOUNT_LOCKED",
                "Account Locked Notification",
                "Your account has been locked - {{appName}}",
                @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Account Locked</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h1 style=""color: #dc3545; font-size: 24px; margin: 0 0 20px 0;"">⚠️ Account Locked</h1>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                Hello {{userName}},
                            </p>
                            <p style=""color: #666666; font-size: 16px; line-height: 1.5; margin: 0 0 20px 0;"">
                                Your {{appName}} account has been locked for security reasons.
                            </p>
                            <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 20px; margin: 20px 0;"">
                                <p style=""color: #856404; font-size: 14px; margin: 0 0 10px 0;"">
                                    <strong>Lock Time:</strong> {{lockTime}}
                                </p>
                                <p style=""color: #856404; font-size: 14px; margin: 0;"">
                                    <strong>Reason:</strong> {{reason}}
                                </p>
                            </div>
                            <p style=""color: #666666; font-size: 14px; line-height: 1.5; margin: 20px 0;"">
                                To unlock your account, please contact our support team at <a href=""mailto:{{supportEmail}}"" style=""color: #007bff;"">{{supportEmail}}</a>
                            </p>
                            <p style=""color: #999999; font-size: 13px; line-height: 1.5; margin: 30px 0 0 0;"">
                                If you believe this is an error, please contact support immediately.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 30px; border-radius: 0 0 8px 8px;"">
                            <p style=""color: #999999; font-size: 12px; line-height: 1.5; margin: 0; text-align: center;"">
                                © 2025 {{appName}}. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                EmailTemplateCategory.System,
                "Email template for account locked notification",
                @"⚠️ ACCOUNT LOCKED

Hello {{userName}},

Your {{appName}} account has been locked for security reasons.

Lock Time: {{lockTime}}
Reason: {{reason}}

To unlock your account, please contact our support team at {{supportEmail}}

If you believe this is an error, please contact support immediately.

© 2025 {{appName}}. All rights reserved.",
                "[\"userName\",\"lockTime\",\"reason\",\"supportEmail\",\"appName\"]",
                true
            )
        };

        await _dbContext.EmailTemplates.AddRangeAsync(templates, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogInfo($"Seeded {templates.Length} email templates successfully");
    }
}
