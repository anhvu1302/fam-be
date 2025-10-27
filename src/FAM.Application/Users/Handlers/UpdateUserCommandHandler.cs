using AutoMapper;
using FAM.Application.DTOs.Users;
using FAM.Application.Users.Commands;
using FAM.Domain.Abstractions;
using FAM.Domain.ValueObjects;
using MediatR;

namespace FAM.Application.Users.Handlers;

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.Id} not found");
        }

        // Check if username is taken by another user
        var isUsernameTaken = await _unitOfWork.Users.IsUsernameTakenAsync(request.Username, request.Id);
        if (isUsernameTaken)
        {
            throw new InvalidOperationException("Username is already taken");
        }

        // Check if email is taken by another user
        var isEmailTaken = await _unitOfWork.Users.IsEmailTakenAsync(request.Email, request.Id);
        if (isEmailTaken)
        {
            throw new InvalidOperationException("Email is already taken");
        }

        // Update user properties
        // Note: In a real implementation, you'd have an Update method on the User entity
        // For now, we'll create a new user with updated values
        var password = string.IsNullOrEmpty(request.Password) ? user.Password : Password.Create(request.Password);

        var updatedUser = FAM.Domain.Users.User.Create(request.Username, request.Email, password.Hash, password.Salt, null, null, null);
        // Copy the ID and audit fields
        typeof(FAM.Domain.Users.User).GetProperty("Id")?.SetValue(updatedUser, request.Id);
        updatedUser.CreatedAt = user.CreatedAt;
        // User does not have CreatedById or UpdatedById as per requirements
        updatedUser.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(updatedUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(updatedUser);
    }
}