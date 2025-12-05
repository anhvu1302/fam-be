using MediatR;

namespace FAM.Application.EmailTemplates.Commands.ActivateEmailTemplate;

/// <summary>
/// Command to activate an email template
/// </summary>
public sealed record ActivateEmailTemplateCommand(long Id) : IRequest<bool>;
