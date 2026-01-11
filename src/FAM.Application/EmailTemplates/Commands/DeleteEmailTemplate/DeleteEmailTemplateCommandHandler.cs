using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Commands.DeleteEmailTemplate;

public sealed class DeleteEmailTemplateCommandHandler : IRequestHandler<DeleteEmailTemplateCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByIdAsync(request.Id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException(ErrorCodes.EMAIL_TEMPLATE_NOT_FOUND, "EmailTemplate", request.Id);
        }

        template.SoftDelete();
        _unitOfWork.EmailTemplates.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
