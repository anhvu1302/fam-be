using FAM.Application.Users.Shared;

namespace FAM.Application.Users.Commands.UpdateUser;

/// <summary>
/// Result of updating a user
/// </summary>
public sealed record UpdateUserResult(UserDto User);
