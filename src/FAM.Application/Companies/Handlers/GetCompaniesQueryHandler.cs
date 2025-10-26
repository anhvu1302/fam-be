using AutoMapper;
using FAM.Application.Common;
using FAM.Application.Companies.DTOs;
using FAM.Application.Companies.Queries;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Companies.Handlers;

/// <summary>
/// Handler for GetCompaniesQuery
/// </summary>
public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginatedResult<CompanyDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly FAM.Application.Common.Options.PagingOptions _pagingOptions;

    public GetCompaniesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, Microsoft.Extensions.Options.IOptions<FAM.Application.Common.Options.PagingOptions> pagingOptions)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _pagingOptions = pagingOptions?.Value ?? new FAM.Application.Common.Options.PagingOptions();
    }

    public async Task<PaginatedResult<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        // Get all companies (in a real implementation, this would be done with proper querying)
        var allCompanies = await _unitOfWork.Companies.GetAllAsync(cancellationToken);
        var companiesQuery = allCompanies.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            companiesQuery = companiesQuery.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                (c.TaxCode != null && c.TaxCode.ToLower().Contains(searchTerm)) ||
                (c.Address != null && c.Address.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        companiesQuery = request.SortOrder?.ToLower() == "asc"
            ? ApplySorting(companiesQuery, request.SortBy, true)
            : ApplySorting(companiesQuery, request.SortBy, false);

        // Get total count
        var totalDocs = companiesQuery.Count();

    // Apply pagination (respect configured max page size)
    var limit = request.Limit <= 0 ? 1 : Math.Min(request.Limit, _pagingOptions.MaxPageSize);
    var skip = (request.Page - 1) * limit;
    var companies = companiesQuery.Skip(skip).Take(limit).ToList();

        // Calculate pagination info
    var totalPages = (int)Math.Ceiling((double)totalDocs / limit);
        var hasPrevPage = request.Page > 1;
        var hasNextPage = request.Page < totalPages;

        return new PaginatedResult<CompanyDto>
        {
            Data = _mapper.Map<IEnumerable<CompanyDto>>(companies),
            TotalDocs = totalDocs,
            Limit = limit,
            HasPrevPage = hasPrevPage,
            HasNextPage = hasNextPage,
            Page = request.Page,
            TotalPages = totalPages,
            Offset = skip,
            PrevPage = hasPrevPage ? request.Page - 1 : null,
            NextPage = hasNextPage ? request.Page + 1 : null,
            PagingCounter = skip + 1,
            Meta = null
        };
    }

    private IQueryable<FAM.Domain.Companies.Company> ApplySorting(IQueryable<FAM.Domain.Companies.Company> query, string? sortBy, bool ascending)
    {
        return sortBy?.ToLower() switch
        {
            "name" => ascending
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name),
            "taxcode" => ascending
                ? query.OrderBy(c => c.TaxCode)
                : query.OrderByDescending(c => c.TaxCode),
            "address" => ascending
                ? query.OrderBy(c => c.Address)
                : query.OrderByDescending(c => c.Address),
            "createdat" => ascending
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt),
            _ => ascending
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };
    }
}