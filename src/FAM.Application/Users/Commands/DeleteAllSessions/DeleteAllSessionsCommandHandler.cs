using FAM.Domain.Abstractions;
using MediatR;

namespace FAM.Application.Users.Commands.DeleteAllSessions;

public class DeleteAllSessionsCommandHandler : IRequestHandler<DeleteAllSessionsCommand, Unit>
{
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAllSessionsCommandHandler(IUserDeviceRepository userDeviceRepository, IUnitOfWork unitOfWork)
    {
        _userDeviceRepository = userDeviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteAllSessionsCommand request, CancellationToken cancellationToken)
    {
        await _userDeviceRepository.DeactivateAllUserDevicesAsync(
            request.UserId,
            request.ExcludeDeviceId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}