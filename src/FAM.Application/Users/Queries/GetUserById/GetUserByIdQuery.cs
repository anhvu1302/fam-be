using FAM.Application.Users.Shared;
using MediatR;

namespace FAM.Application.Users.Queries.GetUserById;

/// <summary>
/// Query to get a user by ID
/// </summary>
public sealed record GetUserByIdQuery(
    long Id,
    string? Include = null
) : IRequest<UserDto?>;