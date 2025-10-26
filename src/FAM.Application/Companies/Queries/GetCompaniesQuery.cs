using FAM.Application.Common;
using FAM.Application.Companies.DTOs;
using MediatR;

namespace FAM.Application.Companies.Queries;

/// <summary>
/// Query to get paginated list of companies with filtering and sorting
/// </summary>
public class GetCompaniesQuery : IRequest<PaginatedResult<CompanyDto>>
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}