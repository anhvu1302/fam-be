using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Commands.ActivateEmailTemplate;

public sealed class ActivateEmailTemplateCommandHandler : IRequestHandler<ActivateEmailTemplateCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ActivateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException(ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND, "EmailTemplate", request.Id);
        }

        template.Activate();
        _unitOfWork.EmailTemplates.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
