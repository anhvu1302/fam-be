using FAM.Application.Auth.Shared;
using FAM.Application.Auth.VerifyEmailOtp;
using WebApiContracts = FAM.WebApi.Contracts.Auth;

namespace FAM.WebApi.Mappers;

/// <summary>
/// Extension methods for mapping between Application layer DTOs and WebApi contract responses
/// </summary>
public static class AuthMappers
{
    #region User Info Mapping

    /// <summary>
    /// Convert Application UserInfoDto to WebApi UserInfoResponse
    /// </summary>
    public static WebApiContracts.UserInfoResponse ToUserInfoResponse(this UserInfoDto dto)
    {
        return new WebApiContracts.UserInfoResponse(
            Id: dto.Id,
            Username: dto.Username,
            Email: dto.Email,
            FirstName: dto.FirstName,
            LastName: dto.LastName,
            FullName: dto.FullName,
            Avatar: dto.Avatar,
            PhoneNumber: dto.PhoneNumber,
            PhoneCountryCode: dto.PhoneCountryCode,
            DateOfBirth: dto.DateOfBirth,
            Bio: dto.Bio,
            IsEmailVerified: dto.IsEmailVerified,
            PreferredLanguage: dto.PreferredLanguage,
            TimeZone: dto.TimeZone,
            TwoFactorEnabled: dto.IsTwoFactorEnabled
        );
    }

    #endregion

    #region Login Response Mapping

    /// <summary>
    /// Convert Application LoginResponse to WebApi LoginResponse
    /// </summary>
    public static WebApiContracts.LoginResponse ToLoginResponse(this LoginResponse dto)
    {
        return new WebApiContracts.LoginResponse(
            AccessToken: dto.AccessToken,
            RefreshToken: dto.RefreshToken,
            ExpiresIn: dto.ExpiresIn,
            TokenType: dto.TokenType,
            User: dto.User.ToUserInfoResponse(),
            RequiresTwoFactor: dto.RequiresTwoFactor,
            TwoFactorSessionToken: dto.TwoFactorSessionToken,
            RequiresEmailVerification: dto.RequiresEmailVerification,
            MaskedEmail: dto.MaskedEmail
        );
    }

    #endregion

    #region 2FA Response Mapping

    /// <summary>
    /// Convert Application VerifyTwoFactorResponse to WebApi VerifyTwoFactorResponse
    /// </summary>
    public static WebApiContracts.VerifyTwoFactorResponse ToVerifyTwoFactorResponse(this VerifyTwoFactorResponse dto)
    {
        return new WebApiContracts.VerifyTwoFactorResponse(
            AccessToken: dto.AccessToken,
            RefreshToken: dto.RefreshToken,
            ExpiresIn: dto.ExpiresIn,
            TokenType: dto.TokenType,
            User: dto.User.ToUserInfoResponse()
        );
    }

    #endregion

    #region Password Reset Response Mapping

    /// <summary>
    /// Convert Application ForgotPasswordResponse to WebApi ForgotPasswordResponse
    /// </summary>
    public static WebApiContracts.ForgotPasswordResponse ToForgotPasswordResponse(this ForgotPasswordResponse dto)
    {
        return new WebApiContracts.ForgotPasswordResponse(
            MaskedEmail: dto.MaskedEmail
        );
    }

    /// <summary>
    /// Convert Application ResetPasswordResponse to WebApi ResetPasswordResponse
    /// </summary>
    public static WebApiContracts.ResetPasswordResponse ToResetPasswordResponse(this ResetPasswordResponse dto)
    {
        return new WebApiContracts.ResetPasswordResponse();
    }

    /// <summary>
    /// Convert Application VerifyResetTokenResponse to WebApi VerifyResetTokenResponse
    /// </summary>
    public static WebApiContracts.VerifyResetTokenResponse ToVerifyResetTokenResponse(this VerifyResetTokenResponse dto)
    {
        return new WebApiContracts.VerifyResetTokenResponse(
            MaskedEmail: dto.MaskedEmail
        );
    }

    #endregion

    #region Authentication Methods Response Mapping

    /// <summary>
    /// Convert Application AuthenticationMethodsResponse to WebApi AuthenticationMethodsResponse
    /// </summary>
    public static WebApiContracts.AuthenticationMethodsResponse ToAuthenticationMethodsResponse(
        this AuthenticationMethodsResponse dto)
    {
        return new WebApiContracts.AuthenticationMethodsResponse(
            EmailAuthenticationEnabled: dto.EmailAuthenticationEnabled,
            MaskedEmail: dto.MaskedEmail,
            TwoFactorAuthenticatorEnabled: dto.TwoFactorAuthenticatorEnabled,
            TwoFactorSetupDate: dto.TwoFactorSetupDate,
            RecoveryCodesConfigured: dto.RecoveryCodesConfigured,
            RemainingRecoveryCodes: dto.RemainingRecoveryCodes,
            AvailableMethods: dto.AvailableMethods?.Select(m => m.ToAuthenticationMethodInfo()).ToList()
        );
    }

    /// <summary>
    /// Convert Application AuthenticationMethodInfo to WebApi AuthenticationMethodInfo
    /// </summary>
    public static WebApiContracts.AuthenticationMethodInfo ToAuthenticationMethodInfo(
        this AuthenticationMethodInfo dto)
    {
        return new WebApiContracts.AuthenticationMethodInfo(
            MethodType: dto.MethodType,
            DisplayName: dto.DisplayName,
            IsEnabled: dto.IsEnabled,
            IsPrimary: dto.IsPrimary,
            AdditionalInfo: dto.AdditionalInfo
        );
    }

    /// <summary>
    /// Convert Application VerifyEmailOtpLoginResponse to WebApi VerifyEmailOtpResponse
    /// </summary>
    public static WebApiContracts.VerifyEmailOtpResponse ToVerifyEmailOtpResponse(this VerifyEmailOtpLoginResponse dto)
    {
        return new WebApiContracts.VerifyEmailOtpResponse(
            AccessToken: dto.AccessToken,
            RefreshToken: dto.RefreshToken,
            ExpiresIn: dto.ExpiresIn,
            TokenType: dto.TokenType,
            User: dto.User.ToUserInfoResponse()
        );
    }

    /// <summary>
    /// Convert Application VerifyTwoFactorResponse to WebApi VerifyRecoveryCodeResponse
    /// (VerifyRecoveryCodeCommand returns VerifyTwoFactorResponse)
    /// </summary>
    public static WebApiContracts.VerifyRecoveryCodeResponse ToVerifyRecoveryCodeResponse(this VerifyTwoFactorResponse dto)
    {
        return new WebApiContracts.VerifyRecoveryCodeResponse(
            AccessToken: dto.AccessToken,
            RefreshToken: dto.RefreshToken,
            ExpiresIn: dto.ExpiresIn,
            TokenType: dto.TokenType,
            User: dto.User.ToUserInfoResponse()
        );
    }

    #endregion
}
