using FAM.Application.Common;
using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;

using MediatR;

namespace FAM.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Result<CreateUserResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateUserResult>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check if username is taken
        var isUsernameTaken = await _unitOfWork.Users.IsUsernameTakenAsync(request.Username);
        if (isUsernameTaken) return Result<CreateUserResult>.Failure("Username is already taken", ErrorType.Conflict);

        // Check if email is taken
        var isEmailTaken = await _unitOfWork.Users.IsEmailTakenAsync(request.Email);
        if (isEmailTaken) return Result<CreateUserResult>.Failure("Email is already taken", ErrorType.Conflict);

        // Create user
        var user = User.Create(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = user.ToUserDto();
        var result = new CreateUserResult(dto!);

        return Result<CreateUserResult>.Success(result);
    }
}
