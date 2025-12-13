using FAM.Application.Common;

using MediatR;

namespace FAM.Application.Users.Commands.UpdateUser;

/// <summary>
/// Command to update an existing user
/// </summary>
public sealed record UpdateUserCommand(
    long Id,
    string? Username = null,
    string? Email = null,
    string? Password = null,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? Bio = null,
    DateTime? DateOfBirth = null,
    string? PreferredLanguage = null,
    string? TimeZone = null,
    bool? ReceiveNotifications = null,
    bool? ReceiveMarketingEmails = null
) : IRequest<Result<UpdateUserResult>>;
