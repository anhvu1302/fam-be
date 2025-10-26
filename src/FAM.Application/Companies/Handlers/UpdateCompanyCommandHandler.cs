using AutoMapper;
using FAM.Application.Companies.Commands;
using FAM.Application.Companies.DTOs;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Companies.Handlers;

/// <summary>
/// Handler for UpdateCompanyCommand
/// </summary>
public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(request.Id, cancellationToken);
        if (company == null)
        {
            throw new KeyNotFoundException($"Company with ID {request.Id} not found");
        }

        // Check if company name is taken by another company
        var isNameTaken = await _unitOfWork.Companies.IsNameTakenAsync(request.Name, request.Id);
        if (isNameTaken)
        {
            throw new InvalidOperationException("Company name is already taken");
        }

        // Update company
        company.Update(request.Name, request.TaxCode, request.Address);
        company.UpdatedAt = DateTime.UtcNow;
        company.UpdatedById = request.UpdatedBy;

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}