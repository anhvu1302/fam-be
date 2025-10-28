using FAM.Application.DTOs.Users;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using MediatR;

namespace FAM.Application.Users.Queries;

/// <summary>
/// Query to get devices for a specific user
/// Endpoint: GET /api/users/{userId}/devices
/// </summary>
public sealed record GetUserDevicesQuery : IRequest<PageResult<UserDeviceDto>>
{
    public long UserId { get; init; }
    public string? Filter { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
