using FAM.Application.Querying;
using FAM.Application.Querying.Parsing;

namespace FAM.Application.Abstractions;

/// <summary>
/// Service để query entities với Filter DSL
/// </summary>
public interface IQueryService<TDto> where TDto : class
{
    Task<PageResult<TDto>> QueryAsync(QueryRequest request, CancellationToken cancellationToken = default);
}
