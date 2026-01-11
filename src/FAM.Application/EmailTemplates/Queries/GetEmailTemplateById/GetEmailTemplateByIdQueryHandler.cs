using FAM.Application.EmailTemplates.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplateById;

public sealed class GetEmailTemplateByIdQueryHandler : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmailTemplateByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
        {
            return null;
        }

        return MapToDto(template);
    }

    private static EmailTemplateDto MapToDto(EmailTemplate template)
    {
        return new EmailTemplateDto
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Subject = template.Subject,
            HtmlBody = template.HtmlBody,
            PlainTextBody = template.PlainTextBody,
            Description = template.Description,
            AvailablePlaceholders = template.AvailablePlaceholders,
            IsActive = template.IsActive,
            IsSystem = template.IsSystem,
            Category = (int)template.Category,
            CategoryName = template.Category.ToString(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}
