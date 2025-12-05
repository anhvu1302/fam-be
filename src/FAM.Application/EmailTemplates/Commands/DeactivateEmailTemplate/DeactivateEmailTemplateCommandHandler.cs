using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using MediatR;

namespace FAM.Application.EmailTemplates.Commands.DeactivateEmailTemplate;

public sealed class DeactivateEmailTemplateCommandHandler : IRequestHandler<DeactivateEmailTemplateCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeactivateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.EmailTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
            throw new NotFoundException(ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND, "EmailTemplate", request.Id);

        template.Deactivate();
        _unitOfWork.EmailTemplates.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
