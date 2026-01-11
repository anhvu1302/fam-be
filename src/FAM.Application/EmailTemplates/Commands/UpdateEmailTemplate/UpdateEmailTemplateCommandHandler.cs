using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Commands.UpdateEmailTemplate;

public sealed class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException(ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND, "EmailTemplate", request.Id);
        }

        template.Update(
            request.Name,
            request.Subject,
            request.HtmlBody,
            (EmailTemplateCategory)request.Category,
            request.Description,
            request.PlainTextBody,
            request.AvailablePlaceholders
        );

        _unitOfWork.EmailTemplates.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
