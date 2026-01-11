using FAM.Application.EmailTemplates.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;

using MediatR;

namespace FAM.Application.EmailTemplates.Queries.GetEmailTemplateByCode;

public sealed class GetEmailTemplateByCodeQueryHandler : IRequestHandler<GetEmailTemplateByCodeQuery, EmailTemplateDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmailTemplateByCodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByCodeQuery request,
        CancellationToken cancellationToken)
    {
        EmailTemplate? template = await _unitOfWork.EmailTemplates.GetByCodeAsync(request.Code, cancellationToken);
        if (template == null)
        {
            return null;
        }

        return template.ToDto();
    }
}
