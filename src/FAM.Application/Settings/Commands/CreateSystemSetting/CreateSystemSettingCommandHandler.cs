using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Commands.CreateSystemSetting;

public sealed class CreateSystemSettingCommandHandler : IRequestHandler<CreateSystemSettingCommand, long>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSystemSettingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        SystemSetting? existingSetting = await _unitOfWork.SystemSettings.GetByKeyAsync(request.Key, cancellationToken);
        if (existingSetting != null)
            throw new ConflictException(
                ErrorCodes.SETTING_KEY_EXISTS,
                "SystemSetting",
                "Key");

        var setting = SystemSetting.Create(
            request.Key,
            request.DisplayName,
            request.Value,
            request.DefaultValue,
            request.DataType,
            request.Group,
            request.Description,
            request.SortOrder,
            request.IsRequired,
            request.IsSensitive
        );

        // Set additional properties
        if (request.ValidationRules != null)
            setting.SetValidationRules(request.ValidationRules);
        if (request.Options != null)
            setting.SetOptions(request.Options);
        if (!request.IsEditable)
            setting.MakeReadOnly();
        if (!request.IsVisible)
            setting.Hide();

        await _unitOfWork.SystemSettings.AddAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return setting.Id;
    }
}
