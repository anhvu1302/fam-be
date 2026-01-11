using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Commands.DeleteSystemSetting;

public sealed class DeleteSystemSettingCommandHandler : IRequestHandler<DeleteSystemSettingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSystemSettingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteSystemSettingCommand request, CancellationToken cancellationToken)
    {
        SystemSetting? setting = await _unitOfWork.SystemSettings.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
        {
            throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, $"Setting with ID {request.Id} not found");
        }

        _unitOfWork.SystemSettings.Delete(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
