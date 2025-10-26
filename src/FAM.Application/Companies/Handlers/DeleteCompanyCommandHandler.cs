using FAM.Application.Companies.Commands;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Companies.Handlers;

/// <summary>
/// Handler for DeleteCompanyCommand
/// </summary>
public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(request.Id, cancellationToken);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Soft delete
        company.SoftDelete(request.DeletedBy);
        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}