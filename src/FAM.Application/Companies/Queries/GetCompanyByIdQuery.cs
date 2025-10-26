using FAM.Application.Companies.DTOs;
using MediatR;

namespace FAM.Application.Companies.Queries;

/// <summary>
/// Query to get a company by ID
/// </summary>
public class GetCompanyByIdQuery : IRequest<CompanyDto>
{
    public long Id { get; set; }
}