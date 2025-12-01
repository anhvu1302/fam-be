namespace FAM.Application.Common.Services;

/// <summary>
/// Service để gửi email
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email OTP cho xác thực 2FA
    /// </summary>
    Task SendOtpEmailAsync(string toEmail, string otpCode, string userName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gửi email reset password
    /// </summary>
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName, string resetUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gửi email xác nhận đổi password thành công
    /// </summary>
    Task SendPasswordChangedEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);
}
