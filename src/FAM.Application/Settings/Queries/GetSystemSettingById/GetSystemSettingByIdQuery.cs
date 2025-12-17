using FAM.Application.Settings.Shared;

using MediatR;

namespace FAM.Application.Settings.Queries.GetSystemSettingById;

/// <summary>
/// Query to get a system setting by ID
/// </summary>
public sealed record GetSystemSettingByIdQuery(long Id) : IRequest<SystemSettingDto?>;
