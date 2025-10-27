using FAM.Application.DTOs.Users;
using MediatR;

namespace FAM.Application.Users.Commands;

/// <summary>
/// Command to update an existing user
/// </summary>
public class UpdateUserCommand : IRequest<UserDto>
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? FullName { get; set; }
    public long? UpdatedBy { get; set; }
}