using MediatR;

namespace FAM.Application.EmailTemplates.Commands.DeleteEmailTemplate;

/// <summary>
/// Command to delete an email template (soft delete)
/// </summary>
public sealed record DeleteEmailTemplateCommand(long Id) : IRequest<bool>;