using FAM.Application.Settings.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Queries.GetSystemSettingById;

public sealed class GetSystemSettingByIdQueryHandler : IRequestHandler<GetSystemSettingByIdQuery, SystemSettingDto?>
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetSystemSettingByIdQueryHandler(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<SystemSettingDto?> Handle(GetSystemSettingByIdQuery request, CancellationToken cancellationToken)
    {
        SystemSetting? setting = await _systemSettingRepository.GetByIdAsync(request.Id, cancellationToken);
        return setting.ToDto();
    }
}
