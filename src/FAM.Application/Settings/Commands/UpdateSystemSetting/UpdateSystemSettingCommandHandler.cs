using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Commands.UpdateSystemSetting;

public sealed class UpdateSystemSettingCommandHandler : IRequestHandler<UpdateSystemSettingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSystemSettingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        SystemSetting? setting = await _unitOfWork.SystemSettings.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
            throw new NotFoundException(ErrorCodes.SETTING_NOT_FOUND, $"Setting with ID {request.Id} not found");

        setting.Update(
            request.DisplayName,
            request.Description,
            request.DefaultValue,
            request.SortOrder
        );

        if (request.Value != null)
            setting.SetValue(request.Value);
        if (request.ValidationRules != null)
            setting.SetValidationRules(request.ValidationRules);
        if (request.Options != null)
            setting.SetOptions(request.Options);
        if (request.IsVisible.HasValue)
        {
            if (request.IsVisible.Value)
                setting.Show();
            else
                setting.Hide();
        }

        if (request.IsEditable.HasValue)
        {
            if (request.IsEditable.Value)
                setting.MakeEditable();
            else
                setting.MakeReadOnly();
        }

        _unitOfWork.SystemSettings.Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
