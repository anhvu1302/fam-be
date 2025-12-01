using FAM.Application.Auth.DTOs;
using MediatR;

namespace FAM.Application.Auth.Commands;

/// <summary>
/// Command để gửi email reset password
/// </summary>
public sealed record ForgotPasswordCommand : IRequest<ForgotPasswordResponse>
{
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Command để verify reset token
/// </summary>
public sealed record VerifyResetTokenCommand : IRequest<VerifyResetTokenResponse>
{
    public string Email { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
}

/// <summary>
/// Command để reset password
/// </summary>
public sealed record ResetPasswordCommand : IRequest<ResetPasswordResponse>
{
    public string Email { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}