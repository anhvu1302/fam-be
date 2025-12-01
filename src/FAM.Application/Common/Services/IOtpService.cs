namespace FAM.Application.Common.Services;

/// <summary>
/// Service để tạo và lưu trữ OTP codes tạm thời - SECURED với session token
/// Sử dụng Redis để cache OTP và rate limiting
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Tạo và lưu OTP code cho user với session token để bảo mật
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="sessionToken">Session token để bind OTP</param>
    /// <param name="expirationMinutes">Thời gian hết hạn (mặc định 10 phút)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OTP code 6 chữ số</returns>
    Task<string> GenerateOtpAsync(long userId, string sessionToken, int expirationMinutes = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify OTP code với session token (đảm bảo OTP đúng với session)
    /// Rate limit: Tối đa 5 lần thử sai
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="sessionToken">Session token để verify</param>
    /// <param name="otpCode">Mã OTP cần verify</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True nếu OTP đúng, False nếu sai hoặc hết hạn</returns>
    Task<bool> VerifyOtpAsync(long userId, string sessionToken, string otpCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa OTP code sau khi verify thành công hoặc hết hạn
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="sessionToken">Session token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveOtpAsync(long userId, string sessionToken, CancellationToken cancellationToken = default);
}