using MediatR;

namespace FAM.Application.Menu.Commands.DeleteMenu;

public record DeleteMenuCommand(long Id) : IRequest;