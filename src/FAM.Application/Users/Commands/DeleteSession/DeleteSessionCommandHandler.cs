using FAM.Domain.Abstractions;
using FAM.Domain.Common;
using MediatR;

namespace FAM.Application.Users.Commands.DeleteSession;

public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, Unit>
{
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSessionCommandHandler(IUserDeviceRepository userDeviceRepository, IUnitOfWork unitOfWork)
    {
        _userDeviceRepository = userDeviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var device = await _userDeviceRepository.GetByIdAsync(request.SessionId, cancellationToken);
        
        if (device == null || device.UserId != request.UserId)
        {
            throw new DomainException(ErrorCodes.USER_SESSION_NOT_FOUND, "Session not found or access denied.");
        }

        device.Deactivate();
        _userDeviceRepository.Update(device);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
