using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Commands.CreateEmailTemplate;

public sealed class CreateEmailTemplateCommandHandler : IRequestHandler<CreateEmailTemplateCommand, long>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        bool codeExists = await _unitOfWork.EmailTemplates.CodeExistsAsync(request.Code, null, cancellationToken);
        if (codeExists)
        {
            throw new ConflictException(ErrorCodes.EMAIL_TEMPLATE_CODE_EXISTS, "EmailTemplate", "Code");
        }

        EmailTemplate template = EmailTemplate.Create(
            request.Code,
            request.Name,
            request.Subject,
            request.HtmlBody,
            (EmailTemplateCategory)request.Category,
            request.Description,
            request.PlainTextBody,
            request.AvailablePlaceholders,
            request.IsSystem
        );

        await _unitOfWork.EmailTemplates.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
