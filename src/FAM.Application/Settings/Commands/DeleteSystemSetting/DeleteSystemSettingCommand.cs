using MediatR;

namespace FAM.Application.Settings.Commands.DeleteSystemSetting;

/// <summary>
/// Command to delete a system setting
/// </summary>
public sealed record DeleteSystemSettingCommand(long Id) : IRequest<Unit>;
