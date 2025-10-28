using AutoMapper;
using FAM.Application.Abstractions;
using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Users;
using FAM.Application.Users.Queries;
using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Users.Handlers;

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IQueryService<UserDto> _queryService;

    public GetUserByIdQueryHandler(IQueryService<UserDto> queryService)
    {
        _queryService = queryService;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Use QueryService with filter by ID to leverage include functionality
        var queryRequest = new QueryRequest
        {
            Filter = $"id == {request.Id}",
            Include = request.Include,
            Page = 1,
            PageSize = 1
        };

        var result = await _queryService.QueryAsync(queryRequest, cancellationToken);
        return result.Items.FirstOrDefault();
    }
}