using FAM.Application.Querying;
using FAM.Application.Users.Shared;
using MediatR;

namespace FAM.Application.Users.Queries.GetUsers;

/// <summary>
/// Query to get paginated list of users with filtering and sorting
/// </summary>
public sealed record GetUsersQuery(QueryRequest QueryRequest)
    : IRequest<PageResult<UserDto>>;