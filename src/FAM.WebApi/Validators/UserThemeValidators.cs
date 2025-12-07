using FAM.Domain.Common;
using FAM.WebApi.Contracts.Users;
using FluentValidation;

namespace FAM.WebApi.Validators;

public class UpdateUserThemeRequestValidator : AbstractValidator<UpdateUserThemeRequest>
{
    private static readonly string[] ValidThemes = { "System", "Light", "Dark", "Leaf", "Blossom", "BlueJelly" };

    public UpdateUserThemeRequestValidator()
    {
        RuleFor(x => x.Theme)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.VAL_REQUIRED)
            .WithMessage(ErrorMessages.GetMessage(ErrorCodes.VAL_REQUIRED))
            .Must(theme => ValidThemes.Contains(theme))
            .WithErrorCode(ErrorCodes.VAL_INVALID_VALUE)
            .WithMessage($"Theme must be one of: {string.Join(", ", ValidThemes)}");

        RuleFor(x => x.PrimaryColor)
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.VAL_TOO_LONG)
            .WithMessage("Primary color must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PrimaryColor));

        RuleFor(x => x.Transparency)
            .InclusiveBetween(0m, 1m)
            .WithErrorCode(ErrorCodes.VAL_OUT_OF_RANGE)
            .WithMessage("Transparency must be between 0.0 and 1.0");

        RuleFor(x => x.BorderRadius)
            .InclusiveBetween(0, 50)
            .WithErrorCode(ErrorCodes.VAL_OUT_OF_RANGE)
            .WithMessage("Border radius must be between 0 and 50 pixels");
    }
}
