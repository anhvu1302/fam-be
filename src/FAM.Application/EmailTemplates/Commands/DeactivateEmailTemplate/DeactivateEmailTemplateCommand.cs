using MediatR;

namespace FAM.Application.EmailTemplates.Commands.DeactivateEmailTemplate;

/// <summary>
/// Command to deactivate an email template
/// </summary>
public sealed record DeactivateEmailTemplateCommand(long Id) : IRequest<bool>;
