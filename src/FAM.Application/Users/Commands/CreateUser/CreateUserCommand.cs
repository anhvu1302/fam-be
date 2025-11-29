using FAM.Application.Common;
using MediatR;

namespace FAM.Application.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public sealed record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null
) : IRequest<Result<CreateUserResult>>;