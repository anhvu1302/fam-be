using MediatR;

namespace FAM.Application.Users.Commands;

/// <summary>
/// Command to delete a user
/// </summary>
public class DeleteUserCommand : IRequest<Unit>
{
    public long Id { get; set; }
    public long? DeletedBy { get; set; }
}