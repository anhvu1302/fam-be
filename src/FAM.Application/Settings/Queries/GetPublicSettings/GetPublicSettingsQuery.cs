using FAM.Application.Settings.Shared;

using MediatR;

namespace FAM.Application.Settings.Queries.GetPublicSettings;

/// <summary>
/// Query to get all public settings (non-sensitive)
/// </summary>
public sealed record GetPublicSettingsQuery : IRequest<List<PublicSettingDto>>;
