using FAM.Application.Querying;
using FAM.Application.Settings.Shared;

using MediatR;

namespace FAM.Application.Settings.Queries.GetSystemSettings;

/// <summary>
/// Query to get paginated list of system settings with filtering and sorting
/// </summary>
public sealed record GetSystemSettingsQuery(QueryRequest QueryRequest)
    : IRequest<PageResult<SystemSettingDto>>;
