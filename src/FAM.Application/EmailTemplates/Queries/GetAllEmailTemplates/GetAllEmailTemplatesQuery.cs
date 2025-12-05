using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetAllEmailTemplates;

/// <summary>
/// Query to get all email templates
/// </summary>
public sealed record GetAllEmailTemplatesQuery : IRequest<IReadOnlyList<EmailTemplateDto>>
{
    public bool? IsActive { get; init; }
    public int? Category { get; init; }
}
