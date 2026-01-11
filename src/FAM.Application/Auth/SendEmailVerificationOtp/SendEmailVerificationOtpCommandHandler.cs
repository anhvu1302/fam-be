using System.Text;

using FAM.Application.Common.Services;

using MediatR;

using Microsoft.Extensions.Logging;

namespace FAM.Application.Auth.SendEmailVerificationOtp;

/// <summary>
/// Handler for sending email verification OTP during login flow
/// Used when email is not verified
/// </summary>
public class SendEmailVerificationOtpCommandHandler
    : IRequestHandler<SendEmailVerificationOtpCommand, SendEmailVerificationOtpResponse>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailVerificationOtpCommandHandler> _logger;

    public SendEmailVerificationOtpCommandHandler(
        IEmailService emailService,
        ILogger<SendEmailVerificationOtpCommandHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SendEmailVerificationOtpResponse> Handle(
        SendEmailVerificationOtpCommand request,
        CancellationToken cancellationToken)
    {
        // Generate 6-digit OTP
        string otp = GenerateOtp();
        TimeSpan otpExpiry = TimeSpan.FromMinutes(10);

        // Send OTP via email
        try
        {
            // Use SendOtpEmailAsync which is already implemented
            await _emailService.SendOtpEmailAsync(
                request.Email,
                otp,
                request.Email,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification OTP email to {Email}", request.Email);
            throw;
        }

        // Create OTP session token 
        // Format: base64(email:expiry)
        DateTime expiryTime = DateTime.UtcNow.AddMinutes(10);
        string otpSessionToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{request.Email}:{expiryTime:O}"));

        return new SendEmailVerificationOtpResponse
        {
            OtpSessionToken = otpSessionToken,
            MaskedEmail = MaskEmail(request.Email),
            ExpiresIn = (int)otpExpiry.TotalSeconds,
            Message = $"Mã xác nhận đã được gửi đến {MaskEmail(request.Email)}"
        };
    }

    /// <summary>
    /// Generate random 6-digit OTP
    /// </summary>
    private static string GenerateOtp()
    {
        Random random = new();
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Mask email address: user@email.com -> u***@email.com
    /// </summary>
    private static string MaskEmail(string email)
    {
        string[] parts = email.Split('@');
        if (parts.Length != 2)
        {
            return email;
        }

        string localPart = parts[0];
        string domain = parts[1];

        if (localPart.Length <= 2)
        {
            return $"{localPart}***@{domain}";
        }

        string masked = localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];
        return $"{masked}@{domain}";
    }
}
