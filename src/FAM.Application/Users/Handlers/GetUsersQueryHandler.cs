using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Users.Queries;
using MediatR;

namespace FAM.Application.Users.Handlers;

/// <summary>
/// Handler for GetUsersQuery - Uses advanced Filter DSL via IQueryService
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PageResult<UserDto>>
{
    private readonly IQueryService<UserDto> _queryService;

    public GetUsersQueryHandler(IQueryService<UserDto> queryService)
    {
        _queryService = queryService;
    }

    public async Task<PageResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Convert to QueryRequest and delegate to query service
        var queryRequest = request.ToQueryRequest();
        return await _queryService.QueryAsync(queryRequest, cancellationToken);
    }
}