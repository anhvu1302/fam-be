using FAM.Application.Common;
using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;

using MediatR;

namespace FAM.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, Result<UpdateUserResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateUserResult>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            return Result<UpdateUserResult>.Failure($"User with ID {request.Id} not found", ErrorType.NotFound);

        // Update personal info if provided
        user.UpdatePersonalInfo(
            request.FirstName ?? user.FirstName,
            request.LastName ?? user.LastName,
            user.Avatar,
            request.Bio ?? user.Bio,
            request.DateOfBirth ?? user.DateOfBirth
        );

        // Update contact info if phone provided
        if (request.PhoneNumber != null) user.UpdateContactInfo(request.PhoneNumber);

        // Update password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.UpdatePassword(request.Password);
        }

        // Update preferences if provided
        user.UpdatePreferences(
            request.PreferredLanguage ?? user.PreferredLanguage,
            request.TimeZone ?? user.TimeZone,
            request.ReceiveNotifications ?? user.ReceiveNotifications,
            request.ReceiveMarketingEmails ?? user.ReceiveMarketingEmails
        );

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = user.ToUserDto();
        var result = new UpdateUserResult(dto!);

        return Result<UpdateUserResult>.Success(result);
    }
}
