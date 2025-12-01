using FAM.Domain.Common;
using FAM.WebApi.Contracts.Auth;
using FluentValidation;

namespace FAM.WebApi.Validators;

/// <summary>
/// Validator for login requests - reads rules from DomainRules
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Identity)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_IDENTITY_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_IDENTITY_REQUIRED))
            .MinimumLength(DomainRules.Username.MinLength)
            .WithErrorCode(ErrorCodes.VAL_IDENTITY_TOO_SHORT)
            .WithMessage($"Username or email must be at least {DomainRules.Username.MinLength} characters")
            .MaximumLength(DomainRules.Email.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_IDENTITY_TOO_LONG)
            .WithMessage($"Username or email must not exceed {DomainRules.Email.MaxLength} characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_PASSWORD_REQUIRED))
            .MinimumLength(DomainRules.Password.MinLength)
            .WithErrorCode(ErrorCodes.VAL_PASSWORD_TOO_SHORT)
            .WithMessage($"Password must be at least {DomainRules.Password.MinLength} characters")
            .MaximumLength(DomainRules.Password.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_PASSWORD_TOO_LONG)
            .WithMessage($"Password must not exceed {DomainRules.Password.MaxLength} characters");
    }
}

/// <summary>
/// Validator for 2FA verification requests
/// </summary>
public sealed class VerifyTwoFactorRequestValidator : AbstractValidator<VerifyTwoFactorRequest>
{
    public VerifyTwoFactorRequestValidator()
    {
        RuleFor(x => x.TwoFactorCode)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_2FA_CODE_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_CODE_REQUIRED))
            .Length(6).WithErrorCode(ErrorCodes.VAL_2FA_CODE_INVALID_LENGTH)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_CODE_INVALID_LENGTH))
            .Matches(@"^\d{6}$").WithErrorCode(ErrorCodes.VAL_2FA_CODE_INVALID_FORMAT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_CODE_INVALID_FORMAT));

        RuleFor(x => x.TwoFactorSessionToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_2FA_SESSION_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_SESSION_REQUIRED));
    }
}

/// <summary>
/// Validator for refresh token requests
/// </summary>
public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_REFRESH_TOKEN_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_REFRESH_TOKEN_REQUIRED));
    }
}

/// <summary>
/// Validator for change password requests
/// </summary>
public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_CURRENT_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_CURRENT_PASSWORD_REQUIRED));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_REQUIRED))
            .MinimumLength(DomainRules.Password.MinLength)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_TOO_SHORT)
            .WithMessage($"New password must be at least {DomainRules.Password.MinLength} characters")
            .MaximumLength(DomainRules.Password.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_TOO_LONG)
            .WithMessage($"New password must not exceed {DomainRules.Password.MaxLength} characters")
            .Must(DomainRules.Password.HasUppercase)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_UPPERCASE)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_UPPERCASE))
            .Must(DomainRules.Password.HasLowercase)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_LOWERCASE)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_LOWERCASE))
            .Must(DomainRules.Password.HasDigit)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_DIGIT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_DIGIT))
            .Must(DomainRules.Password.HasSpecialChar)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_SPECIAL)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_SPECIAL));
    }
}

/// <summary>
/// Validator for enable 2FA requests
/// </summary>
public sealed class Enable2FARequestValidator : AbstractValidator<Enable2FARequest>
{
    public Enable2FARequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_PASSWORD_REQUIRED));
    }
}

/// <summary>
/// Validator for confirm 2FA requests
/// </summary>
public sealed class Confirm2FARequestValidator : AbstractValidator<Confirm2FARequest>
{
    public Confirm2FARequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_2FA_CODE_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_CODE_REQUIRED))
            .Length(6).WithErrorCode(ErrorCodes.VAL_2FA_CODE_INVALID_LENGTH)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_CODE_INVALID_LENGTH))
            .Matches(@"^\d{6}$").WithErrorCode(ErrorCodes.VAL_2FA_CODE_INVALID_FORMAT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_CODE_INVALID_FORMAT));
    }
}

/// <summary>
/// Validator for disable 2FA requests
/// </summary>
public sealed class Disable2FARequestValidator : AbstractValidator<Disable2FARequest>
{
    public Disable2FARequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_PASSWORD_REQUIRED));
    }
}

/// <summary>
/// Validator for disable 2FA with backup code requests
/// </summary>
public sealed class DisableTwoFactorWithBackupRequestValidator : AbstractValidator<DisableTwoFactorWithBackupRequest>
{
    public DisableTwoFactorWithBackupRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_USERNAME_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_USERNAME_REQUIRED))
            .MinimumLength(DomainRules.Username.MinLength)
            .WithErrorCode(ErrorCodes.VAL_USERNAME_TOO_SHORT)
            .WithMessage($"Username must be at least {DomainRules.Username.MinLength} characters")
            .MaximumLength(DomainRules.Username.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_USERNAME_TOO_LONG)
            .WithMessage($"Username must not exceed {DomainRules.Username.MaxLength} characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_PASSWORD_REQUIRED));

        RuleFor(x => x.BackupCode)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_BACKUP_CODE_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_BACKUP_CODE_REQUIRED));
    }
}

/// <summary>
/// Validator for select authentication method requests
/// </summary>
public sealed class SelectAuthenticationMethodRequestValidator : AbstractValidator<SelectAuthenticationMethodRequest>
{
    public SelectAuthenticationMethodRequestValidator()
    {
        RuleFor(x => x.TwoFactorSessionToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_2FA_SESSION_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_SESSION_REQUIRED));

        RuleFor(x => x.SelectedMethod)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_AUTH_METHOD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_AUTH_METHOD_REQUIRED))
            .Must(m => m is "email_otp" or "authenticator_app" or "recovery_code")
            .WithErrorCode(ErrorCodes.VAL_AUTH_METHOD_INVALID)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_AUTH_METHOD_INVALID));
    }
}

/// <summary>
/// Validator for verify email OTP requests
/// </summary>
public sealed class VerifyEmailOtpRequestValidator : AbstractValidator<VerifyEmailOtpRequest>
{
    public VerifyEmailOtpRequestValidator()
    {
        RuleFor(x => x.TwoFactorSessionToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_2FA_SESSION_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_SESSION_REQUIRED));

        RuleFor(x => x.EmailOtp)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_EMAIL_OTP_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_OTP_REQUIRED))
            .Length(6).WithErrorCode(ErrorCodes.VAL_EMAIL_OTP_INVALID_LENGTH)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_OTP_INVALID_LENGTH))
            .Matches(@"^\d{6}$").WithErrorCode(ErrorCodes.VAL_EMAIL_OTP_INVALID_FORMAT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_OTP_INVALID_FORMAT));
    }
}

/// <summary>
/// Validator for verify recovery code requests
/// </summary>
public sealed class VerifyRecoveryCodeRequestValidator : AbstractValidator<VerifyRecoveryCodeRequest>
{
    public VerifyRecoveryCodeRequestValidator()
    {
        RuleFor(x => x.TwoFactorSessionToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_2FA_SESSION_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_2FA_SESSION_REQUIRED));

        RuleFor(x => x.RecoveryCode)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_RECOVERY_CODE_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_RECOVERY_CODE_REQUIRED))
            .MinimumLength(8).WithErrorCode(ErrorCodes.VAL_RECOVERY_CODE_TOO_SHORT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_RECOVERY_CODE_TOO_SHORT))
            .MaximumLength(50).WithErrorCode(ErrorCodes.VAL_RECOVERY_CODE_TOO_LONG)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_RECOVERY_CODE_TOO_LONG));
    }
}

/// <summary>
/// Validator for forgot password requests
/// </summary>
public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_EMAIL_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_REQUIRED))
            .MaximumLength(DomainRules.Email.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_TOO_LONG)
            .WithMessage($"Email must not exceed {DomainRules.Email.MaxLength} characters")
            .Must(DomainRules.Email.IsValidFormat)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_INVALID)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_INVALID));
    }
}

/// <summary>
/// Validator for reset password requests
/// </summary>
public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_EMAIL_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_REQUIRED))
            .MaximumLength(DomainRules.Email.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_TOO_LONG)
            .WithMessage($"Email must not exceed {DomainRules.Email.MaxLength} characters")
            .Must(DomainRules.Email.IsValidFormat)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_INVALID)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_INVALID));

        RuleFor(x => x.ResetToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_RESET_TOKEN_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_RESET_TOKEN_REQUIRED));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_REQUIRED))
            .MinimumLength(DomainRules.Password.MinLength)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_TOO_SHORT)
            .WithMessage($"Password must be at least {DomainRules.Password.MinLength} characters")
            .MaximumLength(DomainRules.Password.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_TOO_LONG)
            .WithMessage($"Password must not exceed {DomainRules.Password.MaxLength} characters")
            .Must(DomainRules.Password.HasUppercase)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_UPPERCASE)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_UPPERCASE))
            .Must(DomainRules.Password.HasLowercase)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_LOWERCASE)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_LOWERCASE))
            .Must(DomainRules.Password.HasDigit)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_DIGIT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_DIGIT))
            .Must(DomainRules.Password.HasSpecialChar)
            .WithErrorCode(ErrorCodes.VAL_NEW_PASSWORD_NO_SPECIAL)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_NEW_PASSWORD_NO_SPECIAL));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_CONFIRM_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_CONFIRM_PASSWORD_REQUIRED))
            .Equal(x => x.NewPassword).WithErrorCode(ErrorCodes.VAL_PASSWORDS_MISMATCH)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_PASSWORDS_MISMATCH));
    }
}

/// <summary>
/// Validator for verify reset token requests
/// </summary>
public sealed class VerifyResetTokenRequestValidator : AbstractValidator<VerifyResetTokenRequest>
{
    public VerifyResetTokenRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_EMAIL_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_REQUIRED))
            .MaximumLength(DomainRules.Email.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_TOO_LONG)
            .WithMessage($"Email must not exceed {DomainRules.Email.MaxLength} characters")
            .Must(DomainRules.Email.IsValidFormat)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_INVALID)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_INVALID));

        RuleFor(x => x.ResetToken)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_RESET_TOKEN_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_RESET_TOKEN_REQUIRED));
    }
}