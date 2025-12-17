using FAM.WebApi.Contracts.EmailTemplates;

using FluentValidation;

namespace FAM.WebApi.Validators;

/// <summary>
/// Validator for create email template requests
/// </summary>
public sealed class CreateEmailTemplateRequestValidator : AbstractValidator<CreateEmailTemplateRequest>
{
    public CreateEmailTemplateRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("VAL_CODE_REQUIRED")
            .WithMessage("Template code is required")
            .MinimumLength(3).WithErrorCode("VAL_CODE_TOO_SHORT")
            .WithMessage("Code must be at least 3 characters")
            .MaximumLength(100).WithErrorCode("VAL_CODE_TOO_LONG")
            .WithMessage("Code must not exceed 100 characters")
            .Matches(@"^[A-Z][A-Z0-9_]*$").WithErrorCode("VAL_CODE_INVALID_FORMAT")
            .WithMessage("Code must be uppercase letters, numbers, and underscores only, starting with a letter");

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("VAL_NAME_REQUIRED")
            .WithMessage("Template name is required")
            .MinimumLength(3).WithErrorCode("VAL_NAME_TOO_SHORT")
            .WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithErrorCode("VAL_NAME_TOO_LONG")
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Subject)
            .NotEmpty().WithErrorCode("VAL_SUBJECT_REQUIRED")
            .WithMessage("Subject is required")
            .MinimumLength(3).WithErrorCode("VAL_SUBJECT_TOO_SHORT")
            .WithMessage("Subject must be at least 3 characters")
            .MaximumLength(500).WithErrorCode("VAL_SUBJECT_TOO_LONG")
            .WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.HtmlBody)
            .NotEmpty().WithErrorCode("VAL_HTMLBODY_REQUIRED")
            .WithMessage("HTML body is required")
            .MinimumLength(10).WithErrorCode("VAL_HTMLBODY_TOO_SHORT")
            .WithMessage("HTML body must be at least 10 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithErrorCode("VAL_CATEGORY_REQUIRED")
            .WithMessage("Category is required")
            .InclusiveBetween(1, 4).WithErrorCode("VAL_CATEGORY_INVALID")
            .WithMessage("Category must be between 1 and 4");

        RuleFor(x => x.PlainTextBody)
            .MaximumLength(50000).WithErrorCode("VAL_PLAINTEXTBODY_TOO_LONG")
            .WithMessage("Plain text body cannot exceed 50000 characters")
            .When(x => !string.IsNullOrEmpty(x.PlainTextBody));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithErrorCode("VAL_DESCRIPTION_TOO_LONG")
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AvailablePlaceholders)
            .MaximumLength(2000).WithErrorCode("VAL_PLACEHOLDERS_TOO_LONG")
            .WithMessage("Available placeholders cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.AvailablePlaceholders));
    }
}

/// <summary>
/// Validator for update email template requests
/// </summary>
public sealed class UpdateEmailTemplateRequestValidator : AbstractValidator<UpdateEmailTemplateRequest>
{
    public UpdateEmailTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("VAL_NAME_REQUIRED")
            .WithMessage("Template name is required")
            .MinimumLength(3).WithErrorCode("VAL_NAME_TOO_SHORT")
            .WithMessage("Name must be at least 3 characters")
            .MaximumLength(200).WithErrorCode("VAL_NAME_TOO_LONG")
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Subject)
            .NotEmpty().WithErrorCode("VAL_SUBJECT_REQUIRED")
            .WithMessage("Subject is required")
            .MinimumLength(3).WithErrorCode("VAL_SUBJECT_TOO_SHORT")
            .WithMessage("Subject must be at least 3 characters")
            .MaximumLength(500).WithErrorCode("VAL_SUBJECT_TOO_LONG")
            .WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.HtmlBody)
            .NotEmpty().WithErrorCode("VAL_HTMLBODY_REQUIRED")
            .WithMessage("HTML body is required")
            .MinimumLength(10).WithErrorCode("VAL_HTMLBODY_TOO_SHORT")
            .WithMessage("HTML body must be at least 10 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithErrorCode("VAL_CATEGORY_REQUIRED")
            .WithMessage("Category is required")
            .InclusiveBetween(1, 4).WithErrorCode("VAL_CATEGORY_INVALID")
            .WithMessage("Category must be between 1 and 4");

        RuleFor(x => x.PlainTextBody)
            .MaximumLength(50000).WithErrorCode("VAL_PLAINTEXTBODY_TOO_LONG")
            .WithMessage("Plain text body cannot exceed 50000 characters")
            .When(x => !string.IsNullOrEmpty(x.PlainTextBody));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithErrorCode("VAL_DESCRIPTION_TOO_LONG")
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AvailablePlaceholders)
            .MaximumLength(2000).WithErrorCode("VAL_PLACEHOLDERS_TOO_LONG")
            .WithMessage("Available placeholders cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.AvailablePlaceholders));
    }
}
