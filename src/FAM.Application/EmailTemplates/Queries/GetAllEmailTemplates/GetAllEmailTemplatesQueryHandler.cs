using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetAllEmailTemplates;

public sealed class GetAllEmailTemplatesQueryHandler : IRequestHandler<GetAllEmailTemplatesQuery, IReadOnlyList<EmailTemplateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEmailTemplatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> Handle(GetAllEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<EmailTemplate> templates;

        if (request.Category.HasValue)
        {
            templates = await _unitOfWork.EmailTemplates.GetByCategoryAsync(
                (EmailTemplateCategory)request.Category.Value,
                cancellationToken);
        }
        else if (request.IsActive == true)
        {
            templates = await _unitOfWork.EmailTemplates.GetActiveTemplatesAsync(cancellationToken);
        }
        else
        {
            templates = await _unitOfWork.EmailTemplates.GetAllAsync(cancellationToken);
        }

        // Apply IsActive filter if specified and not already filtered
        if (request.IsActive.HasValue && !request.Category.HasValue)
        {
            templates = templates.Where(t => t.IsActive == request.IsActive.Value);
        }

        return templates.Select(MapToDto).ToList();
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
