using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Users;

using FluentValidation;

namespace FAM.WebApi.Validators;

/// <summary>
/// Validator for create user requests - reads rules from DomainRules
/// </summary>
public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_USERNAME_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_USERNAME_REQUIRED))
            .MinimumLength(DomainRules.Username.MinLength)
            .WithErrorCode(ErrorCodes.VAL_USERNAME_TOO_SHORT)
            .WithMessage($"Username must be at least {DomainRules.Username.MinLength} characters")
            .MaximumLength(DomainRules.Username.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_USERNAME_TOO_LONG)
            .WithMessage($"Username must not exceed {DomainRules.Username.MaxLength} characters")
            .Must(DomainRules.Username.IsValidFormat)
            .WithErrorCode(ErrorCodes.USER_INVALID_USERNAME)
            .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_EMAIL_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_REQUIRED))
            .MaximumLength(DomainRules.Email.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_TOO_LONG)
            .WithMessage($"Email must not exceed {DomainRules.Email.MaxLength} characters")
            .Must(DomainRules.Email.IsValidFormat)
            .WithErrorCode(ErrorCodes.VAL_EMAIL_INVALID)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_INVALID));

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_PASSWORD_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_PASSWORD_REQUIRED))
            .MinimumLength(DomainRules.Password.MinLength)
            .WithErrorCode(ErrorCodes.VAL_PASSWORD_TOO_SHORT)
            .WithMessage($"Password must be at least {DomainRules.Password.MinLength} characters")
            .MaximumLength(DomainRules.Password.MaxLength)
            .WithErrorCode(ErrorCodes.VAL_PASSWORD_TOO_LONG)
            .WithMessage($"Password must not exceed {DomainRules.Password.MaxLength} characters")
            .Must(DomainRules.Password.HasUppercase)
            .WithErrorCode(ErrorCodes.VO_PASSWORD_NO_UPPERCASE)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VO_PASSWORD_NO_UPPERCASE))
            .Must(DomainRules.Password.HasLowercase)
            .WithErrorCode(ErrorCodes.VO_PASSWORD_NO_LOWERCASE)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VO_PASSWORD_NO_LOWERCASE))
            .Must(DomainRules.Password.HasDigit)
            .WithErrorCode(ErrorCodes.VO_PASSWORD_NO_DIGIT)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VO_PASSWORD_NO_DIGIT))
            .Must(DomainRules.Password.HasSpecialChar)
            .WithErrorCode(ErrorCodes.VO_PASSWORD_NO_SPECIAL)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VO_PASSWORD_NO_SPECIAL));

        // Optional fields
        When(x => x.FirstName != null, () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("First name must not exceed 50 characters");
        });

        When(x => x.LastName != null, () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(50).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("Last name must not exceed 50 characters");
        });

        When(x => x.PhoneNumber != null, () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Must(phone => phone == null || DomainRules.PhoneNumber.IsValidLength(phone))
                .WithErrorCode(ErrorCodes.USER_INVALID_PHONE)
                .WithMessage(
                    $"Phone number must be between {DomainRules.PhoneNumber.MinLength} and {DomainRules.PhoneNumber.MaxLength} digits");
        });
    }
}

/// <summary>
/// Validator for update user requests - reads rules from DomainRules
/// </summary>
public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        // All fields are optional in update, but if provided must be valid
        When(x => x.Username != null, () =>
        {
            RuleFor(x => x.Username)
                .MinimumLength(DomainRules.Username.MinLength)
                .WithErrorCode(ErrorCodes.VAL_USERNAME_TOO_SHORT)
                .WithMessage($"Username must be at least {DomainRules.Username.MinLength} characters")
                .MaximumLength(DomainRules.Username.MaxLength)
                .WithErrorCode(ErrorCodes.VAL_USERNAME_TOO_LONG)
                .WithMessage($"Username must not exceed {DomainRules.Username.MaxLength} characters")
                .Must(username => username == null || DomainRules.Username.IsValidFormat(username))
                .WithErrorCode(ErrorCodes.USER_INVALID_USERNAME)
                .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");
        });

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email)
                .MaximumLength(DomainRules.Email.MaxLength)
                .WithErrorCode(ErrorCodes.VAL_EMAIL_TOO_LONG)
                .WithMessage($"Email must not exceed {DomainRules.Email.MaxLength} characters")
                .Must(email => email == null || DomainRules.Email.IsValidFormat(email))
                .WithErrorCode(ErrorCodes.VAL_EMAIL_INVALID)
                .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_EMAIL_INVALID));
        });

        When(x => x.FirstName != null, () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("First name must not exceed 50 characters");
        });

        When(x => x.LastName != null, () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(50).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("Last name must not exceed 50 characters");
        });

        When(x => x.PhoneNumber != null, () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Must(phone => phone == null || DomainRules.PhoneNumber.IsValidLength(phone))
                .WithErrorCode(ErrorCodes.USER_INVALID_PHONE)
                .WithMessage(
                    $"Phone number must be between {DomainRules.PhoneNumber.MinLength} and {DomainRules.PhoneNumber.MaxLength} digits");
        });

        When(x => x.Bio != null, () =>
        {
            RuleFor(x => x.Bio)
                .MaximumLength(500).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("Bio must not exceed 500 characters");
        });

        When(x => x.PreferredLanguage != null, () =>
        {
            RuleFor(x => x.PreferredLanguage)
                .MaximumLength(10).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("Language code must not exceed 10 characters");
        });

        When(x => x.TimeZone != null, () =>
        {
            RuleFor(x => x.TimeZone)
                .MaximumLength(50).WithErrorCode(ErrorCodes.VAL_TOO_LONG)
                .WithMessage("Timezone must not exceed 50 characters");
        });
    }
}

/// <summary>
/// Validator for update avatar requests
/// </summary>
public sealed class UpdateAvatarRequestValidator : AbstractValidator<UpdateAvatarRequest>
{
    public UpdateAvatarRequestValidator()
    {
        RuleFor(x => x.UploadId)
            .NotEmpty().WithErrorCode(ErrorCodes.VAL_REQUIRED)
            .WithMessage("Upload ID is required");
    }
}
