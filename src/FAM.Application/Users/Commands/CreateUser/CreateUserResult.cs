using FAM.Application.Users.Shared;

namespace FAM.Application.Users.Commands.CreateUser;

/// <summary>
/// Result of creating a user
/// </summary>
public sealed record CreateUserResult(UserDto User);
