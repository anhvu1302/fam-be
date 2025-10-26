using FAM.Application.DTOs.Users;
using MediatR;

namespace FAM.Application.Users.Queries;

/// <summary>
/// Query to get a user by ID
/// </summary>
public class GetUserByIdQuery : IRequest<UserDto?>
{
    public long Id { get; set; }
}