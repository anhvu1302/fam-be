using FAM.Application.Settings.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Entities;

using MediatR;

namespace FAM.Application.Settings.Queries.GetPublicSettings;

public sealed class GetPublicSettingsQueryHandler : IRequestHandler<GetPublicSettingsQuery, List<PublicSettingDto>>
{
    private readonly ISystemSettingRepository _systemSettingRepository;

    public GetPublicSettingsQueryHandler(ISystemSettingRepository systemSettingRepository)
    {
        _systemSettingRepository = systemSettingRepository;
    }

    public async Task<List<PublicSettingDto>> Handle(GetPublicSettingsQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<SystemSetting> settings = await _systemSettingRepository.GetAllAsync(cancellationToken);
        return settings
            .Where(s => !s.IsSensitive)
            .Select(s => s.ToPublicDto()!)
            .ToList();
    }
}
