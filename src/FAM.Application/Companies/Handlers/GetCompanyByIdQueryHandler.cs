using AutoMapper;
using FAM.Application.Companies.DTOs;
using FAM.Application.Companies.Queries;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Companies.Handlers;

/// <summary>
/// Handler for GetCompanyByIdQuery
/// </summary>
public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCompanyByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(request.Id, cancellationToken);
        return company != null ? _mapper.Map<CompanyDto>(company) : null;
    }
}