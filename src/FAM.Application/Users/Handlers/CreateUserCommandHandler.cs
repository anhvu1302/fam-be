using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Application.Users.Commands;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;
using FAM.Domain.ValueObjects;
using MediatR;

namespace FAM.Application.Users.Handlers;

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username is taken
        var isUsernameTaken = await _unitOfWork.Users.IsUsernameTakenAsync(request.Username);
        if (isUsernameTaken)
        {
            throw new InvalidOperationException("Username is already taken");
        }

        // Check if email is taken
        var isEmailTaken = await _unitOfWork.Users.IsEmailTakenAsync(request.Email);
        if (isEmailTaken)
        {
            throw new InvalidOperationException("Email is already taken");
        }

        // Create user
        var user = User.Create(request.Username, request.Email, request.Password, null, null, null);
        // User does not have CreatedById as per requirements

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}