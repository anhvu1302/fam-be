using AutoMapper;
using FAM.Application.Companies.Commands;
using FAM.Application.Companies.DTOs;
using FAM.Domain.Abstractions;
using FAM.Domain.Companies;
using MediatR;

namespace FAM.Application.Companies.Handlers;

/// <summary>
/// Handler for CreateCompanyCommand
/// </summary>
public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        // Check if company name is taken
        var isNameTaken = await _unitOfWork.Companies.IsNameTakenAsync(request.Name);
        if (isNameTaken)
        {
            throw new InvalidOperationException("Company name is already taken");
        }

        // Create company
        var company = Company.Create(request.Name, request.TaxCode, request.Address);
        company.CreatedById = request.CreatedBy;

        await _unitOfWork.Companies.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}